using HotelBooking.application.Services.Helpers.Media.BusinessLicenses;
using HotelBooking.application.Services.Domains.Media.BusinessLicenses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using V1.Models;

namespace HotelBooking.api.Controllers.V1.Media.BusinessLicenses
{
    [Route("api/v1/files")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("business-licenses")]
        [RequestSizeLimit(BusinessLicenseFileRules.MaximumSize + 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = BusinessLicenseFileRules.MaximumSize + 1024 * 1024)]
        public async Task<IActionResult> UploadBusinessLicense(IFormFile? file, CancellationToken cancellationToken = default)
        {
            // FR-ONB-011: reject missing input before opening a caller-owned multipart stream.
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var ownerId) || ownerId <= 0)
                return Unauthorized();
            if (file == null)
                return ApiResponseHandlerHelper.HandleResponse(ResponseFactory.Failure<UploadResultDTO>(
                    StatusCodeResponse.BadRequest, "A business license file is required."));

            await using var stream = file.OpenReadStream();
            var result = new UploadFileDTO
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                Content = stream
            };

            var response = await _fileService.UploadBusinessLicenseAsync(result, ownerId, cancellationToken);
            return ApiResponseHandlerHelper.HandleResponse(response);

        }

    }
}
