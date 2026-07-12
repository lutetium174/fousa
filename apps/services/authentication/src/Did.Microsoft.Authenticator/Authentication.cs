using System.Text;
using System.Text.Json;
using Engine;
using Microsoft.Extensions.Configuration;

namespace Did.Microsoft.Authenticator;

public class Authentication(ILoginState loginStates)
{
    public async Task<string> Request(IConfiguration  config)
    {
        var endpoint = config["Vc:RequestEndpoint"]!;
        var callbackUrl = config["Vc:CallbackUrl"]!;
        var contract = config["Vc:VerificationContract"]!;

        var state = Guid.NewGuid();
        loginStates.SetState(state, false);

        var body = new
        {
            includeQRCode = true,
            callback = new
            {
                url = callbackUrl,
                state
            },
            registration = new
            {
                clientName = "Verified ID Login"
            },
            authority = contract,
            verification = new
            {
                credentialType = "YourCredentialType"
            }
        };

        var token = await Tokeniser.GetAccessToken(config);

        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var json = JsonSerializer.Serialize(body);
        var response = await client.PostAsync(
            endpoint,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        return await response.Content.ReadAsStringAsync();
    }

    public async Task Callback(Stream payload)
    {
        using var reader = new StreamReader(payload);
        var body = await reader.ReadToEndAsync();

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        var state = Guid.Parse(root.GetProperty("state").GetString() ?? string.Empty);
        var code = root.GetProperty("code").GetString();

        if (code == "verified")
        {
            loginStates.SetState(state, true);
        }
    }

    public bool Verify(Guid state) => loginStates.GetState(state);
}