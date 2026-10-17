using System.Net.Http.Json;
using Mediator;

namespace Did.WaltId.Verifier.Handlers;

public class VerificationHandler(IHttpClientFactory httpClientFactory) : IRequestHandler<Verify, Verified>
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(WaltVerifierClient));

    public async ValueTask<Verified> Handle(Verify request, CancellationToken cancellationToken)
    {
        var response = await _client.PostAsJsonAsync(
            "/v1/verifier2-api/requests",
            request,
            cancellationToken);
        
        return await  response.Content.ReadFromJsonAsync<Verified>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}

public record Verify : IRequest<Verified>
{
    public VerificationQuery Query { get; set; }
    public string CallbackUrl { get; set; }
}

public class VerificationQuery
{
    public string Type { get; set; } = "dcql";
    public object Query { get; set; }
}

public record Verified
{
    public string RequestUri { get; set; }
    public string QrCode { get; set; }
}