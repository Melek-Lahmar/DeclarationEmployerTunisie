namespace DeclarationEmployer.Contracts.Common;

public sealed class ApiResponse<T>
{
    public bool Success { get; set; }

    public T? Data { get; set; }

    public ApiError? Error { get; set; }

    public string? Message { get; set; }

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> Fail(ApiError error, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error,
            Message = message
        };
    }
}
