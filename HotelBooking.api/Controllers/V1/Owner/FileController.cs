using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HotelBooking.application.Services.Domains.Media;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using V1.Models;

namespace HotelBooking.api.Controllers.V1.Owner
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
        public async Task<IActionResult> UploadBusinessLicense(IFormFile file)
        {
            var claim = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (claim == 0) return Unauthorized();

            var result = new UploadFileDTO
            {
                FileName = file.FileName,
                Size = file.Length,
                Content = file.OpenReadStream()
            };

            var response = await _fileService.UploadBusinessLicenseAsync(result, claim);
            return ApiResponseHandlerHelper.HandleResponse(response);

        }

    }
}