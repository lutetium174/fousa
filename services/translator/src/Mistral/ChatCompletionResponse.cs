using System.Text.Json.Serialization;

namespace Mistral;

/// <summary>
/// Response from chat completion.
/// </summary>
public class ChatCompletionResponse
{
    /// <summary>
    /// Unique identifier for the completion.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The model used for completion.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// The created timestamp.
    /// </summary>
    public long? Created { get; set; }

    /// <summary>
    /// The completion message.
    /// </summary>
    public List<Choice>? Choices { get; set; }
    
    /// <summary>
    /// Total tokens used.
    /// </summary>
    public int? TotalTokens { get; set; }

    /// <summary>
    /// Evaluation count.
    /// </summary>
    public int? EvalCount { get; set; }

    public class Choice
    {
        public string FinishReason { get; set; }
        public int Index { get; set; }
        public ChatMessage Message { get; set; }
    }
}