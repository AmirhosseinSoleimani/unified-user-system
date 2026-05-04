namespace UnifiedUserSystem.src.Contracts.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public object? Errors { get; init; }

        public static ApiResponse<T> Ok(T? data, string? message = null)
            => new()
            {
                Success = true,
                Data = data,
                Message = message
            };

        public static ApiResponse<T> Fail(string message, object? errors = null)
            => new()
            {
                Success = false,
                Message = message,
                Errors = errors
            };
    }
}
