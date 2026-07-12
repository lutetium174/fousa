namespace Mistral;

/// <summary>
/// Configuration for the Mistral client.
/// </summary>
public class MistralOptions
{
    public const string Mistral = "Mistral";
    /// <summary>
    /// The base URI of the Mistral API
    /// </summary>
    public required string BaseUri { get; set; }
    public string? Instruction { get; set; }
    public float? Temperature { get; set; } = 0f;
    public int? MaxTokens { get; set; } = 10;
}