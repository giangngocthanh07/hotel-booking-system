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
        // 1. SUCCESS GROUP (2xx)
        // ==========================================
        public static ApiResponse<T> Success<T>(T? data, string? message)
        {
            return CreateResponse(StatusCodeResponse.Success, message, data);
        }

        // ==========================================
        // 2. FAILURE GROUP (Client/Logic errors - 4xx)
        // Includes: BadRequest, NotFound, Conflict, Unauthorized...
        // ==========================================
        public static ApiResponse<T> Failure<T>(string? statusCode, string? message)
        {
            // You can add validation here to ensure statusCode is 4xx if needed
            return CreateResponse<T>(statusCode, message, default!);
        }

        // ==========================================
        // 3. SERVER ERROR GROUP (System errors - 5xx)
        // ==========================================
        [Obsolete]
        public static ApiResponse<T> ServerError<T>()
        {
            return CreateResponse<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER, default!);
        }

        // ==========================================
        // PRIVATE METHOD (Core) — avoids code duplication (DRY)
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