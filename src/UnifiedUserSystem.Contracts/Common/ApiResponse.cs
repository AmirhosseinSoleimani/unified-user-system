namespace UnifiedUserSystem.src.Contracts.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public string? Code { get; init; }
        public string? TraceId { get; init; }
        public object? Errors { get; init; }

        public static ApiResponse<T> Ok(
            T? data,
            string? message = null,
            string? code = null
            )
            => new ()
            {
                Success = true,
                Data = data,
                Message = message,
                Code = code
            };

        public static ApiResponse<T> Fail(
            string message,
            object? errors = null,
            string? code = null,
            string? traceId = null
            )
            => new()
            {
                Success = false,
                Message = message,
                Code = code,
                TraceId = traceId,
                Errors = errors
            };
    }
}
