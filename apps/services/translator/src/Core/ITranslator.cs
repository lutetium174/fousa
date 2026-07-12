using System.Globalization;
using Foundation;

namespace Core;

public interface ITranslator
{
    Task<Result<string>> Translate(
        string? input,
        CultureInfo culture,
        CancellationToken cancellationToken);
}