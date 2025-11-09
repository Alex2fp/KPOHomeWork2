namespace TaskPlanner.Application.Common;

public record OperationResult(bool Success, string? Error = null)
{
    public static OperationResult Ok() => new(true);
    public static OperationResult Fail(string error) => new(false, error);
}

public record OperationResult<T>(bool Success, T? Value, string? Error = null)
{
    public static OperationResult<T> Ok(T value) => new(true, value);
    public static OperationResult<T> Fail(string error) => new(false, default, error);
}
