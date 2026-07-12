namespace Mistral;

/// <summary>
/// Request for chat completion.
/// </summary>
public class ChatCompletionRequest
{
    /// <summary>
    /// ID of the model to use.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The messages to send to the model.
    /// </summary>
    public List<ChatMessage> Messages { get; set; } = new();

    /// <summary>
    /// Sampling temperature (0.0 to 1.0).
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Whether to stream the response.
    /// </summary>
    public bool Stream { get; set; } = false;
}