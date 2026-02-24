namespace LinkVault.Application.Common.Results;

public enum ResultErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}

public record ResultError(
    ResultErrorType Type,
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null
);

public class Result
{
    protected Result(bool isSuccess, ResultError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ResultError? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(ResultError error) => new(false, error);
}

public class Result<T> : Result
{
    private Result(bool isSuccess, T? value, ResultError? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, null);

    public new static Result<T> Failure(ResultError error) => new(false, default, error);
}
