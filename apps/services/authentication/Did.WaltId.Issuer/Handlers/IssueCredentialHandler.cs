using System.Net.Http.Json;
using Mediator;

namespace Did.WaltId.Issuer.Handlers;

public class IssueCredentialHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<IssueCredential, CredentialIssued>
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(WaltIssuerClient));

    public async ValueTask<CredentialIssued> Handle(IssueCredential request, CancellationToken cancellationToken)
    {
        var response = await _client.PostAsJsonAsync(
            "/v1/issuer-api/credentials/issue",
            new
            {
                credentialData = new
                {
                    type = new[] { "VerifiableCredential", "FousaAccount" },
                    credentialSubject = new
                    {
                    }
                }
            },
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<CredentialIssued>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}

public record IssueCredential : IRequest<CredentialIssued>
{
}

public record CredentialIssued
{
    public string Credential { get; set; }
}