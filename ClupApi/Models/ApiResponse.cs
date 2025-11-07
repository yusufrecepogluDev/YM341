namespace ClupApi.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string[] Errors { get; set; } = Array.Empty<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, string[] errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }

        public static ApiResponse<T> ValidationErrorResponse(string[] errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResponse(string message = "Operation completed successfully")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Timestamp = DateTime.UtcNow
            };
        }

        public static new ApiResponse ErrorResponse(string message, string[] errors)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }

        public static new ApiResponse ValidationErrorResponse(string[] errors)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Validation failed",
                Errors = errors,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}