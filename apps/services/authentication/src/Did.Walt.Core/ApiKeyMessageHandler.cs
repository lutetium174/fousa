namespace Did.Walt.Core;

public class ApiKeyMessageHandler(string apiKey) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(apiKey))
            request.Headers.Authorization = new("Bearer", apiKey);
        return base.SendAsync(request, cancellationToken);
    }
}
