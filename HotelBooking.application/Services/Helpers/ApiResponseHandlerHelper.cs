using Microsoft.AspNetCore.Mvc;
using HotelBooking.application.Helpers;

namespace HotelBooking.api.Helpers
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
}