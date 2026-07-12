using System.Net.Http.Json;
using Mediator;

namespace Did.WaltId.Issuer.Handlers;

public class CreateTemplateHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<CreateTemplate, TemplateCreated>
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(nameof(WaltIssuerClient));

    public async ValueTask<TemplateCreated> Handle(CreateTemplate request, CancellationToken cancellationToken)
    {
        var response = await _client.PostAsJsonAsync(
            "/v1/issuer-api/credentials/templates",
            request,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<TemplateCreated>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}

public record CreateTemplate : IRequest<TemplateCreated>
{
    public string Id { get; set; }
    public string Format { get; set; } = "jwt_vc_json";
    public string[] Type { get; set; }
    public Dictionary<string, object> CredentialSubject { get; set; }
}

public record TemplateCreated
{
    public string Id { get; set; }
}