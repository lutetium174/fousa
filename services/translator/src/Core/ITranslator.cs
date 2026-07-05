using Foundation;

namespace Core;

public interface ITranslator
{
    Task<Result<string>> Translate(
        string input,
        string language,
        CancellationToken cancellationToken);
}