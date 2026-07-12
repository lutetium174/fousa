using System.Net.Http.Json;
using Mediator;

namespace Did.WaltId.Issuer.Handlers;

public class CredentialOfferHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<CredentialOffer, CredentialOfferResponse>
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(WaltIssuerClient));

    public async ValueTask<CredentialOfferResponse> Handle(
        CredentialOffer request,
        CancellationToken cancellationToken)
    {
        var response = await _client.PostAsJsonAsync(
            "/v1/issuer-api/oidc/credential-offers",
            request,
            cancellationToken);
        
        return await response.Content.ReadFromJsonAsync<CredentialOfferResponse>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}

public record CredentialOffer : IRequest<CredentialOfferResponse>
{
    public string[] Credentials { get; set; }
    public object Grants { get; set; }
}

public record CredentialOfferResponse
{
    public string OfferUri { get; set; }
    public string QrCode { get; set; }
}