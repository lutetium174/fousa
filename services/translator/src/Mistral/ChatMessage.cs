namespace Mistral;

/// <summary>
/// A single message in a chat conversation.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// The role of the message author.
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The content of the message.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}