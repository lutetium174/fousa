namespace Foundation;

public record Error<T>(string Message)
{
    public static implicit operator Error<T>(string? message) 
        => new(message ?? $"{typeof(T).Name} error");
}