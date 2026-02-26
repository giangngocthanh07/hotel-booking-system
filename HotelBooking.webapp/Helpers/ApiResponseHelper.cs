public static class ResponseFactory
{
    // ==========================================
    // 1. NHÓM SUCCESS (Thành công - 2xx)
    // ==========================================
    public static ApiResponse<T> Success<T>(T? data, string? message)
    {
        return CreateResponse(StatusCodeResponse.Success, message, data);
    }

    // ==========================================
    // 2. NHÓM FAILURE (Lỗi logic/Client - 4xx)
    // Bao gồm: BadRequest, NotFound, Conflict, Unauthorized...
    // ==========================================
    public static ApiResponse<T> Failure<T>(string? statusCode, string? message)
    {
        // Bạn có thể thêm validation ở đây để đảm bảo statusCode là 4xx nếu muốn
        return CreateResponse<T>(statusCode, message, default!);
    }

    // ==========================================
    // 3. NHÓM SERVER ERROR (Lỗi hệ thống - 5xx)
    // ==========================================
    public static ApiResponse<T> ServerError<T>()
    {
        return CreateResponse<T>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER, default!);
    }

    // ==========================================
    // HÀM PRIVATE (Core) - Để tránh lặp code (DRY)
    // ==========================================
    private static ApiResponse<T> CreateResponse<T>(string? statusCode, string? message, T? content)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Message = message,
            Content = content
        };
    }
}

