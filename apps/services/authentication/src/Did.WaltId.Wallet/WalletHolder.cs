using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Did.Walt.Core;
using Microsoft.Extensions.Options;

namespace Did.WaltId.Wallet;

public class WalletHolder(IOptions<WaltIdOptions> options, IHttpClientFactory httpClientFactory)
{
    private readonly SemaphoreSlim _sessionLock = new(1);
    private string? _token;
    private string? _walletId;

    public async Task<string> CreateDid()
    {
        var (client, walletId) = await EnsureSessionAsync();
        
        using var keys = await client.GetAsync($"/wallet/{walletId}/keys");
        keys.EnsureSuccessStatusCode();
        using var keysDoc = JsonDocument.Parse(await keys.Content.ReadAsStringAsync());
        var hasKeys = keysDoc.RootElement.ValueKind == JsonValueKind.Array
                      && keysDoc.RootElement.GetArrayLength() > 0;

        if (!hasKeys)
        {
            using var generated = await client.PostAsJsonAsync(
                $"/wallet/{walletId}/keys/generate",
                new { backend = "jwk", keyType = "Ed25519" });
            generated.EnsureSuccessStatusCode();
        }

        using var response = await client.PostAsJsonAsync(
            $"/wallet/{walletId}/dids/create",
            new { method = "key" });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> ListCredentials()
    {
        var (client, walletId) = await EnsureSessionAsync();
        using var response = await client.GetAsync($"/wallet/{walletId}/credentials");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> Receive(string offerUrl)
    {
        var (client, walletId) = await EnsureSessionAsync();
        using var response = await client.PostAsJsonAsync(
            $"/wallet/{walletId}/credentials/receive",
            new { offerUrl });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> Present(string requestUrl)
    {
        var (client, walletId) = await EnsureSessionAsync();
        using var response = await client.PostAsJsonAsync(
            $"/wallet/{walletId}/credentials/present",
            new { requestUrl });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<(HttpClient Client, string WalletId)> EnsureSessionAsync()
    {
        if (_token is not null && _walletId is not null)
        {
            var cached = httpClientFactory.CreateClient("WalletApi");
            cached.DefaultRequestHeaders.Authorization = new("Bearer", _token);
            return (cached, _walletId);
        }

        await _sessionLock.WaitAsync();
        try
        {
            if (_token is not null && _walletId is not null)
            {
                var cached = httpClientFactory.CreateClient("WalletApi");
                cached.DefaultRequestHeaders.Authorization = new("Bearer", _token);
                return (cached, _walletId);
            }

            var config = options.Value;
            var client = httpClientFactory.CreateClient("WalletApi");
            
            await client.PostAsJsonAsync(
                "/auth/register",
                new { email = config.WalletEmail, password = config.WalletPassword });

            using var login = await client.PostAsJsonAsync(
                "/auth/emailpass",
                new { email = config.WalletEmail, password = config.WalletPassword });
            
            login.EnsureSuccessStatusCode();
            using var loginDoc = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            var token = loginDoc.RootElement.GetProperty("token").GetString()
                        ?? throw new InvalidOperationException("Wallet login returned no token.");
            client.DefaultRequestHeaders.Authorization = new("Bearer", token);

            using var wallets = await client.GetAsync("/wallet");
            wallets.EnsureSuccessStatusCode();
            using var walletsDoc = JsonDocument.Parse(await wallets.Content.ReadAsStringAsync());

            var walletId = walletsDoc.RootElement.ValueKind == JsonValueKind.Array
                ? walletsDoc.RootElement.EnumerateArray()
                    .Select(w => w.GetString())
                    .FirstOrDefault(id => !string.IsNullOrEmpty(id))
                : null;

            if (walletId is null)
            {
                using var created = await client.PostAsJsonAsync("/wallet", new { });
                created.EnsureSuccessStatusCode();
                using var createdDoc = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
                walletId = createdDoc.RootElement.GetProperty("walletId").GetString()
                           ?? throw new InvalidOperationException("Wallet creation returned no wallet id.");
            }

            _token = token;
            _walletId = walletId;
            return (client, walletId);
        }
        finally
        {
            _sessionLock.Release();
        }
    }
}
