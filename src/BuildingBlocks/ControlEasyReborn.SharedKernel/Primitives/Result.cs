namespace ControlEasyReborn.SharedKernel.Primitives;

public sealed class Result<T>
{
    public T? Value { get; }
    public bool IsSuccess { get; }
    public string? Error { get; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    private Result(string error, IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        Error = error;
        IsSuccess = false;
        ValidationErrors = validationErrors;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(string error) => new(error);
    public static Result<T> Failure(string error, IReadOnlyDictionary<string, string[]> validationErrors) => new(error, validationErrors);
}

public sealed class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public IReadOnlyDictionary<string, string[]>? ValidationErrors { get; }

    private Result(bool isSuccess, string? error = null, IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(string error, IReadOnlyDictionary<string, string[]> validationErrors) => new(false, error, validationErrors);
}