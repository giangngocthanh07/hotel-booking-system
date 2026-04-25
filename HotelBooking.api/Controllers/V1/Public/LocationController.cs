using HotelBooking.application.Services.Domains.Common;
using Microsoft.AspNetCore.Mvc;
//using V1.Models;

namespace HotelBooking.api.Controllers.V1.Public
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet("get-countries")]
        public async Task<IActionResult> GetAllCountries()
        {
            var response = await _locationService.GetCountriesAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpGet("get-provinces/{countryId}")]
        public async Task<IActionResult> GetProvincesByCountryId(int countryId)
        {
            var result = await _locationService.GetProvincesAsync(countryId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpGet("get-wards/{provinceId}")]
        public async Task<IActionResult> GetWardsByProvinceId(int provinceId)
        {
            var result = await _locationService.GetWardsByProvinceAsync(provinceId);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

    }
}