namespace Foundation;

public record Result<T>(T? Value, Error<T>? Error = null)
{
    public bool IsSuccess => Value is not null;

    public TResult Match<TResult>(
        Func<T, TResult> success,
        Func<Error<T>, TResult> error)
        => IsSuccess ? success(Value!) : error(Error!);

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error<T> error) => new(default, error);
    public static implicit operator Result<T>(string errorMessage) => new(default, errorMessage);
}