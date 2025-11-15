using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using HotelBooking.application.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using HotelBooking.api.Models;

namespace HotelBooking.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        IHotelService _hotelService;
        IFileHelper _fileHelper;
        public HotelController(IHotelService hotelService, IFileHelper fileHelper)
        {
            _hotelService = hotelService;
            _fileHelper = fileHelper;
        }

        [Authorize(Roles = "Owner")]
        [HttpGet("get-owner-dashboard")]
        public async Task<IActionResult> GetOwnerDashboardAsync()
        {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var response = await _hotelService.GetOwnerDashBoard(ownerId);
            return Ok(response);
        }

        // ================= TÌM KIẾM KHÁCH SẠN THEO FILTER SearchForm.razor ================

        [HttpGet("get-search-options")]
        public async Task<IActionResult> GetSearchOptionsAsync([FromQuery] string cityName,
        [FromQuery] DateTime? checkIn,
        [FromQuery] DateTime? checkOut,
        [FromQuery] int? adults,
        [FromQuery] int? children,
        [FromQuery] int? rooms)
        {
            var response = await _hotelService.GetSearchOptionsAsync(cityName, checkIn, checkOut, adults, children, rooms);
            return Ok(response);
        }

        // =============== ĐỌC, THÊM, SỬA, XÓA TIỆN ÍCH CHO KHÁCH SẠN ================
        // [Authorize(Roles = "Admin")]
        [HttpGet("get-all-amenities")]
        public async Task<IActionResult> GetAllAmenitiesAsync()
        {
            var response = await _hotelService.GetAllAmenitiesAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }


        // [Authorize(Roles = "Admin")]
        [HttpPost("create-amenity")]
        public async Task<IActionResult> CreateAmenityAsync([FromBody] AmenityCreateOrUpdateDTO newAmenity)
        {
            var response = await _hotelService.CreateAmenityAsync(newAmenity);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPut("update-amenity/{id}")]
        public async Task<IActionResult> UpdateAmenityAsync(int id, [FromBody] AmenityCreateOrUpdateDTO amenity)
        {

            var response = await _hotelService.UpdateAmenityAsync(id, amenity);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpDelete("delete-amenity/{id}")]
        public async Task<IActionResult> DeleteAmenityAsync(int id)
        {
            var response = await _hotelService.DeleteAmenityAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpGet("get-all-cities")]
        public async Task<IActionResult> GetAllCitiesAsync()
        {
            var response = await _hotelService.GetAllCitiesAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // ================= ĐĂNG KHÁCH SẠN MỚI ================
        [Authorize(Roles = "Owner")]
        [HttpPost("post-new-hotel")]
        public async Task<IActionResult> PostNewHotelAsync([FromForm] CreateHotelRequestDTO newHotelRequest)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return BadRequest("User identifier claim is missing.");
            }
            var ownerId = int.Parse(claim.Value);

            // Map CreateHotelRequestDTO to CreateHotelDTO
            var newHotelDTO = new CreateHotelDTO
            {
                Name = newHotelRequest.Name,
                Address = newHotelRequest.Address,
                Description = newHotelRequest.Description,
                CityId = newHotelRequest.CityId,
                AmenityIds = newHotelRequest.AmenityIds,
                PolicyIds = newHotelRequest.PolicyIds,
                CoverFile = await _fileHelper.ConvertToUploadFileVM(newHotelRequest.CoverFile),
                MainFile = await _fileHelper.ConvertToUploadFileVM(newHotelRequest.MainFile),
                SubFiles = new List<UploadFileDTO>()
            };

            foreach (var subFile in newHotelRequest.SubFiles)
            {
                newHotelDTO.SubFiles!.Add(await _fileHelper.ConvertToUploadFileVM(subFile));
            }

            var result = await _hotelService.PostHotelAsync(newHotelDTO, ownerId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }


        // ================= ĐỌC, THÊM, SỬA, XÓA CHÍNH SÁCH CHO KHÁCH SẠN ================
        [HttpGet("get-all-policy-types")]
        public async Task<IActionResult> GetAllPolicyTypesAsync()
        {
            var response = await _hotelService.GetAllPolicyTypesAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpGet("get-all-policy-by-type/{policyTypeId}")]
        public async Task<IActionResult> GetAllPoliciesByTypeAsync(int policyTypeId)
        {
            var response = await _hotelService.GetAllPoliciesByTypeAsync(policyTypeId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [Authorize(Roles = "Owner")]
        [HttpPost("test-upload-photo-cloudinary")]
        public async Task<IActionResult> TestUploadPhotoCloudinaryAsync(IFormFile file)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || string.IsNullOrEmpty(claim.Value))
            {
                return BadRequest("User identifier claim is missing.");
            }
            var userId = int.Parse(claim.Value);

            var result = new UploadFileDTO
            {
                FileName = file.FileName,
                Size = file.Length,
                Content = file.OpenReadStream()
            };

            var response = await _hotelService.TestUploadImageToCloudinaryAsync(result, userId);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }


    }
}
