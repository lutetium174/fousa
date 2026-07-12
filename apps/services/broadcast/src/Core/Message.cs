namespace Core;

public record Message
(
    string Topic,
    Guid Sender,
    string Content
);
