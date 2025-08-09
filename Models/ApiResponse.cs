namespace HackerNews.Backend.Models;

public class ApiResponse<T>
{
    public T Data { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }

    public ApiResponse(T data, bool success = true, string? message = null)
    {
        Data = data;
        Success = success;
        Message = message;
    }

    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>(data, true, message);
    }

    public static ApiResponse<T> ErrorResponse(T data, string message)
    {
        return new ApiResponse<T>(data, false, message);
    }
}
