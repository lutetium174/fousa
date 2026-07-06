namespace Core.Responses;

public record MessageResponse(
    Guid Id,
    Guid SenderId,
    string Message,
    DateTime CreatedAt
);