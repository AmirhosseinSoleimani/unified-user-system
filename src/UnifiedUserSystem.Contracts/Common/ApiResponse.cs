namespace UnifiedUserSystem.src.Contracts.Common;

public class ApiResponse<T>
{
    public bool Successful { get; init; }
    public object? Data { get; init; }
    public int ResultCode { get; init; }
    public object? Error { get; init; } = new { };

    public static ApiResponse<T> Ok(
        T? data
        )
        => new ()
        {
            Successful = true,
            ResultCode = 0,
            Data = data,
            Error = new { }
        };

    public static ApiResponse<T> Fail(
        string message,
        object? error = null,
        string? traceId = null)
        => Fail(
            title: message,
            description: message,
            resultCode: ApiResultCodes.BusinessError,
            traceId: traceId);

    public static ApiResponse<T> Fail(
        string title,
        string description,
        int resultCode,
        string? traceId = null
        )
        => new()
        {
            Successful = false,
            ResultCode = resultCode,
            Data = new { },
            Error = new
            {
                traceId,
                title,
                description
            }
        };
}


public static class ApiResultCodes
{
    public const int Success = 0;
    public const int BusinessError = 1;
    public const int AccessDenied = 2;
    public const int TokenExpired = 3;
    public const int ServerError = 4;
}
