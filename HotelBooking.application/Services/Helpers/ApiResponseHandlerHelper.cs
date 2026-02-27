using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.application.Helpers
{
    public static class ApiResponseHandlerHelper
    {
        public static IActionResult HandleResponse<T>(ApiResponse<T> response)
        {
            var result = new ObjectResult(response);

            result.StatusCode = response.StatusCode switch
            {
                StatusCodeResponse.Success => StatusCodes.Status200OK,
                StatusCodeResponse.NotFound => StatusCodes.Status404NotFound,
                StatusCodeResponse.Conflict => StatusCodes.Status409Conflict,
                StatusCodeResponse.BadRequest => StatusCodes.Status400BadRequest,
                StatusCodeResponse.Unauthorized => StatusCodes.Status401Unauthorized,
                StatusCodeResponse.Forbidden => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status500InternalServerError
            };
            return result;
        }
    }

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
        [Obsolete]
        public static ApiResponse<T> ServerError<T>()
        {
            return CreateResponse<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER, default!);
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
}