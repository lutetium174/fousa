using System.Net.Http.Json;
using System.Text.Json;
using Did.Walt.Core;
using Engine;
using Mediator;
using Microsoft.Extensions.Options;

namespace Did.WaltId.Wallet.Handlers;

public class ChallengeHandler(
    IOptions<WaltIdOptions> config,
    IHttpClientFactory httpClientFactory,
    ILoginState loginStates
    ) : IRequestHandler<AuthenticationChallenge, ChallengeAccepted>
{
    private readonly WaltIdOptions _config = config.Value;
    private static readonly string[] Value = ["EdDSA"];

    public async ValueTask<ChallengeAccepted> Handle(AuthenticationChallenge request, CancellationToken cancellationToken)
    {
            var state = Guid.NewGuid();
            loginStates.SetState(state, false);
            
            using var client = httpClientFactory.CreateClient("WalletApi");

            var response = await client.PostAsJsonAsync(
                _config.VerifierApi.Url,
                new
                {
                    flow_type = "cross_device",
                    core = new
                    {
                        client_id = "did:web:alliora.pug-bichir.ts.net",
                        redirect_uri = _config.CallbackUrl,
                        scope = "openid",
                        presentation_definition = new
                        {
                            input_descriptors = new[]
                            {
                                new
                                {
                                    id = "my-credential",
                                    format = new { jwt_vc = new { alg = Value } },
                                    constraints = new { fields = Array.Empty<object>() }
                                }
                            }
                        }
                    }
                }, 
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var sessionId = root.GetProperty("id").GetString() ?? Guid.Empty.ToString();
            var authUrl = root.GetProperty("authorizationRequestUrl").GetString() ?? string.Empty;

            return new(state, Guid.Parse(sessionId), authUrl);
        }
}

public record AuthenticationChallenge : IRequest<ChallengeAccepted> { }

public record ChallengeAccepted(Guid State, Guid SessionId, string QrCode);