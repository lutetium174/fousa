using System.Net.Http.Json;

namespace Pinotchio;

public class PinotClient(IHttpClientFactory clientFactory) : IPinotClient
{
    public async Task<string> ExecuteSql(string sql, CancellationToken cancellationToken = default)
    {
        var client = clientFactory.CreateClient("Pinot");
        var response = await client.PostAsJsonAsync("/sql", new { sql }, cancellationToken);
        
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}