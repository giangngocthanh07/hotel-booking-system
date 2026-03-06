namespace HotelBooking.webapp.Helpers.Common;

public static class ResponseFactory
{
    // ==========================================
    // 1. SUCCESS GROUP (2xx Status Codes)
    // ==========================================
    public static ApiResponse<T> Success<T>(T? data, string? message)
    {
        return CreateResponse(StatusCodeResponse.Success, message, data);
    }

    // ==========================================
    // 2. FAILURE GROUP (Logic/Client Errors - 4xx)
    // Includes: BadRequest, NotFound, Conflict, Unauthorized, etc.
    // ==========================================
    public static ApiResponse<T> Failure<T>(string? statusCode, string? message)
    {
        // Optional: Add validation here to ensure statusCode belongs to 4xx range
        return CreateResponse<T>(statusCode, message, default!);
    }

    // ==========================================
    // 3. SERVER ERROR GROUP (System Errors - 5xx)
    // ==========================================
    public static ApiResponse<T> ServerError<T>()
    {
        return CreateResponse<T>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER, default!);
    }

    // ==========================================
    // CORE PRIVATE METHOD - DRY (Don't Repeat Yourself) Implementation
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