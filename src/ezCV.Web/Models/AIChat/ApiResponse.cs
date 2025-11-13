namespace ezCV.Web.Models.AIChat
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

        public static ApiResponse<T> CreateSuccess(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> CreateError(string error, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Error = error,
                Message = message
            };
        }
    }
}
