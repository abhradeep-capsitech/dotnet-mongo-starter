namespace DotnetMongoStarter.Utils
{
    public class ApiResponse<T>
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        // Success response
        public static ApiResponse<T> Success(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>
            {
                Status = "success",
                Data = data,
                Message = message,
                Errors = null
            };
        }

        // Error response
        public static ApiResponse<T> Error(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Status = "error",
                Data = default,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}
