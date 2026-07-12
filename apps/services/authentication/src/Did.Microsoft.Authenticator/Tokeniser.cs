using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace Did.Microsoft.Authenticator;

public class Tokeniser
{
        public static async Task<string> GetAccessToken(IConfiguration config)
        {
            var tenantId = config["Vc:TenantId"]!;
            var clientId = config["Vc:ClientId"]!;
            var clientSecret = config["Vc:ClientSecret"]!;

            var appConfidential = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority($"https://login.microsoftonline.com/{tenantId}")
                .Build();

            var result = await appConfidential
                .AcquireTokenForClient(new[] { "3db474b9-6a0c-4840-96ac-1fceb342124f/.default" })
                .ExecuteAsync();

            return result.AccessToken;
        }
}