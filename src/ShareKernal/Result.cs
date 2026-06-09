namespace ShareKernal;

public enum ErrorType { NotFound, Forbidden, Conflict, Validation, ServiceUnavailable, TooManyRequests }

public record Error(string Code, string Description, ErrorType Type = ErrorType.Validation)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided", ErrorType.NotFound);

    public static implicit operator Result(Error error) => Result.Failure(error);
}

public class Result
{
    public bool IsSuccess { get; }
    public Error Error { get; }

    protected internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
            throw new ArgumentException("One or more validation errors have been detected.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value)
        => new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
        => _value = value;

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The result is not successful.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}
