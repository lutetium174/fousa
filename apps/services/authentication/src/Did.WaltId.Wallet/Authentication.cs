using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Engine;
using Microsoft.Extensions.Configuration;

namespace Did.WaltId.Wallet;

internal class Authentication(
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILoginState loginStates) : IAuthentication
{
    public async Task<string> Register(string did)
    {
        using var client = httpClientFactory.CreateClient("WalletApi");
        var response = await client.PostAsJsonAsync("dids", new { did = did }); 
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<ChallengeResult> Challenge()
    {
        var verifierApi = new Uri(config["Walt:VerifierApi:Url"]!);

        var state = Guid.NewGuid();
        loginStates.SetState(state, false);

        var body = new
        {
            flow_type = "cross_device",
            core_flow = new
            {
                dcql_query = new
                {
                    input_descriptors = new[]
                    {
                        new
                        {
                            id = "my-credential",
                            format = "jwt_vc_json",
                            meta = new { type_values = new[] { new[] { "VerifiableCredential" } } }
                        }
                    }
                }
            }
        };

        using var client = httpClientFactory.CreateClient("WalletApi");
        var json = JsonSerializer.Serialize(body);

        var response = await client.PostAsync(
            new Uri(verifierApi, "verification-session/create"),
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var content = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        var sessionId = root.GetProperty("sessionId").GetString() ?? Guid.Empty.ToString();
        var authUrl = root.GetProperty("fullAuthorizationRequestUrl").GetString() ?? string.Empty;

        return new(state, Guid.Parse(sessionId), authUrl);
    }

    public async Task Validate(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var body = await reader.ReadToEndAsync();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var state = root.TryGetProperty("state", out var s) ? s.GetString() : null;

        if (state != null)
            loginStates.SetState(Guid.Parse(state), true);
    }

    public bool Verify(Guid state) => loginStates.GetState(state);
}
