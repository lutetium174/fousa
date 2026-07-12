namespace Engine;

public interface IAuthentication
{
    Task<string> Register(string did);
    Task<ChallengeResult> Challenge();
    Task Validate(Stream stream);
    bool Verify(Guid state);
}