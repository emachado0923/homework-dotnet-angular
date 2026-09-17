namespace TaskManager.Application.Common;

public enum ResultError
{
    None = 0,
    NotFound,
    Unauthorized,
    Validation,
    Conflict
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public ResultError Error { get; }

    private Result(bool isSuccess, T? value, string? errorMessage, ResultError error)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Error = error;
    }

    public static Result<T> Success(T value) => new(true, value, null, ResultError.None);

    public static Result<T> Failure(string errorMessage, ResultError error) =>
        new(false, default, errorMessage, error);
}
