using System.Security.Claims;
using HotelBooking.application.Services.Domains.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1;

[ApiController]
[Authorize(Roles = "Owner,Admin")]
[Route("api/v1/hotel-registrations/{requestId:int}/business-license")]
[Tags("Business License Documents")]
public class BusinessLicenseDocumentController(IBusinessLicenseAccessService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Download(int requestId, CancellationToken cancellationToken)
    {
        // FR-ONB-013/014: no caller-controlled Owner/Admin flags or IDs enter the authorization context.
        Response.Headers.CacheControl = "no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        if (User.Identity?.IsAuthenticated != true ||
            !int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) || userId <= 0)
            return Unauthorized();
        if (!User.IsInRole("Admin") && !User.IsInRole("Owner")) return Forbid();
        var result = await service.DownloadAsync(new(requestId, userId, User.IsInRole("Admin")), cancellationToken);
        if (result.StatusCode != StatusCodeResponse.Success || result.Content == null)
            return ApiResponseHandlerHelper.HandleResponse(result);
        return File(result.Content.Content, result.Content.ContentType, result.Content.FileName);
    }
}
