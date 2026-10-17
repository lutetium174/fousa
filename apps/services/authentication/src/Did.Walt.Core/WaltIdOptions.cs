namespace Did.Walt.Core;

public class WaltIdOptions
{
    public ApiOptions IssuerApi { get; set; }
    public ApiOptions VerifierApi { get; set; }
    public ApiOptions WalletApi { get; init; } 
    public Uri CallbackUrl { get; init; }
}

public class ApiOptions
{
    public Uri Url { get; init; }
    public string ApiKey { get; init; }
}