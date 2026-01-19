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
        private readonly IHotelService _hotelService;
        private readonly IAmenityManage _amenityService;
        private readonly IPolicyManage _policyService;
        private readonly IServiceManage _serviceManage;
        private readonly IRoomQualityManage _rqManage;
        private readonly IRoomAttributeFacade _roomAttributeFacade;
        private readonly IManagementAdmin _managementAdmin;
        private readonly IFileHelper _fileHelper;
        public HotelController(IHotelService hotelService, IAmenityManage amenityService, IRoomQualityManage rqManage, IPolicyManage policyService, IServiceManage serviceManage, IFileHelper fileHelper, IRoomAttributeFacade roomAttributeFacade, IManagementAdmin managementAdmin)
        {
            _hotelService = hotelService;
            _amenityService = amenityService;
            _rqManage = rqManage;
            _policyService = policyService;
            _serviceManage = serviceManage;
            _fileHelper = fileHelper;
            _roomAttributeFacade = roomAttributeFacade;
            _managementAdmin = managementAdmin;
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

        #region GET MANAGE GROUP DATA
        // ==========================================
        // 1. API LẤY MENU (SIÊU NHẸ)
        // ==========================================
        // URL: api/hotel/manage/menu/Service
        // Nhiệm vụ: Chỉ trả về danh sách Types để Frontend vẽ Dropdown
        [HttpGet("get-manage-menu/{module}")]
        public async Task<IActionResult> GetManageMenu(ManageModuleEnum module)
        {
            // Gọi sang ManagementAdmin (Class vừa refactor xong)
            var result = await _managementAdmin.GetManageMenuAsync(module);
            return Ok(result);
        }

        // ==========================================
        // 2. API LẤY DỮ LIỆU (THEO TỪNG MODULE)
        // ==========================================
        // Tại sao không gom 1 cái generic? -> Để Swagger hiển thị rõ DTO trả về cho từng loại

        // --- A. SERVICE DATA ---
        // URL: api/hotel/get-service-data?typeId=1
        [HttpGet("get-service-data")]
        public async Task<IActionResult> GetServiceData([FromQuery] int? typeId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var request = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };

            // Gọi trực tiếp ServiceManage (Hàm vừa refactor xong)
            var result = await _serviceManage.GetServicesByTypeAsync(typeId, request);
            return Ok(result);
        }

        // --- B. ROOM QUALITY DATA ---
        // URL: api/hotel/get-room-quality-data?typeId=1
        [HttpGet("get-room-quality-data")]
        public async Task<IActionResult> GetRoomQualityData([FromQuery] int? typeId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var request = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };

            var response = await _rqManage.GetRoomQualitiesByTypeAsync(typeId, request);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // --- C. AMENITY DATA ---
        // URL: api/hotel/get-amenity-data?typeId=1
        [HttpGet("get-amenity-data")]
        public async Task<IActionResult> GetAmenityData([FromQuery] int? typeId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var request = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };

            var response = await _amenityService.GetAmenitiesByTypeAsync(typeId, request);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // --- D. POLICY DATA ---
        // URL: api/hotel/get-policy-data?typeId=1
        [HttpGet("get-policy-data")]
        public async Task<IActionResult> GetPolicyData([FromQuery] int? typeId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var request = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };

            var response = await _policyService.GetPoliciesByTypeAsync(typeId, request);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // ==================================================================================
        // 1. API GET PHÂN TRANG (Thay thế cho các hàm GetUnitTypeData cũ khi dùng cho Table)
        // URL: GET /api/hotel/room-attribute/get-paged-data?type=UnitType&pageIndex=1...
        // ==================================================================================
        [HttpGet("room-attribute/get-paged-data")]
        public async Task<IActionResult> GetPagedData(
            [FromQuery] RoomAttributeType type, // Enum: UnitType, BedType, RoomView...
            [FromQuery] int pageIndex = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] int? typeId = null)     // Chỉ dùng cho RoomQuality
        {
            // 1. Tạo request phân trang
            var paging = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };

            // 2. Gọi Facade (Nó tự biết switch-case vào đúng Manager)
            var response = await _roomAttributeFacade.GetPagedByTypeAsync(type, paging, typeId);
            
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpGet("get-unit-type-data")]
        public async Task<IActionResult> GetUnitTypeData()
        {
            var response = await _roomAttributeFacade.UnitTypeManage.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpGet("get-bed-type-data")]
        public async Task<IActionResult> GetBedTypeData()
        {
            var response = await _roomAttributeFacade.BedTypeManage.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpGet("get-room-view-data")]
        public async Task<IActionResult> GetRoomView()
        {
            var response = await _roomAttributeFacade.RoomViewManage.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
        #endregion

        #region MANAGE UNIT TYPE
        // =============== THÊM, SỬA, XÓA LOẠI PHÒNG ================

        // [Authorize(Roles = "Admin")]
        [HttpPost("create-unit-type")]
        public async Task<IActionResult> CreateUnitTypeAsync([FromBody] UnitTypeCreateOrUpdateDTO newUnitType)
        {
            var response = await _roomAttributeFacade.UnitTypeManage.CreateAsync(newUnitType);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPut("update-unit-type/{id}")]
        public async Task<IActionResult> UpdateUnitTypeAsync(int id, [FromBody] UnitTypeCreateOrUpdateDTO ut)
        {

            var response = await _roomAttributeFacade.UnitTypeManage.UpdateAsync(id, ut);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpDelete("delete-unit-type/{id}")]
        public async Task<IActionResult> DeleteUnitTypeAsync(int id)
        {
            var response = await _roomAttributeFacade.UnitTypeManage.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
        #endregion

        #region MANAGE BED TYPE
        // =============== THÊM, SỬA, XÓA LOẠI GIƯỜNG ================

        // [Authorize(Roles = "Admin")]
        [HttpPost("create-bed-type")]
        public async Task<IActionResult> CreateBedTypeAsync([FromBody] BedTypeCreateOrUpdateDTO newBedType)
        {
            var response = await _roomAttributeFacade.BedTypeManage.CreateAsync(newBedType);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPut("update-bed-type/{id}")]
        public async Task<IActionResult> UpdateBedTypeAsync(int id, [FromBody] BedTypeCreateOrUpdateDTO bt)
        {

            var response = await _roomAttributeFacade.BedTypeManage.UpdateAsync(id, bt);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpDelete("delete-bed-type/{id}")]
        public async Task<IActionResult> DeleteBedTypeAsync(int id)
        {
            var response = await _roomAttributeFacade.BedTypeManage.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
        #endregion

        #region MANAGE ROOM VIEW
        // =============== THÊM, SỬA, XÓA LOẠI VIEW PHÒNG ================

        // [Authorize(Roles = "Admin")]
        [HttpPost("create-room-view")]
        public async Task<IActionResult> CreateRoomViewAsync([FromBody] RoomViewCreateOrUpdateDTO newRoomView)
        {
            var response = await _roomAttributeFacade.RoomViewManage.CreateAsync(newRoomView);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPut("update-room-view/{id}")]
        public async Task<IActionResult> UpdateRoomViewAsync(int id, [FromBody] RoomViewCreateOrUpdateDTO rv)
        {

            var response = await _roomAttributeFacade.RoomViewManage.UpdateAsync(id, rv);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpDelete("delete-room-view/{id}")]
        public async Task<IActionResult> DeleteRoomViewAsync(int id)
        {
            var response = await _roomAttributeFacade.RoomViewManage.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
        #endregion

        #region MANAGE ROOM QUALITY
        // =============== THÊM, SỬA, XÓA CHẤT LƯỢNG PHÒNG ================
        //
        [HttpPost("create-room-quality")]
        public async Task<IActionResult> CreateRoomQualityAsync([FromBody] RoomQualityCreateOrUpdateDTO newRoomQuality)
        {
            var response = await _rqManage.CreateAsync(newRoomQuality);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPut("update-room-quality/{id}")]
        public async Task<IActionResult> UpdateRoomQualityAsync(int id, [FromBody] RoomQualityCreateOrUpdateDTO rq)
        {

            var response = await _rqManage.UpdateAsync(id, rq);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpDelete("delete-room-quality/{id}")]
        public async Task<IActionResult> DeleteRoomQualityAsync(int id)
        {
            var response = await _rqManage.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        #endregion

        #region MANAGE AMENITY
        // =============== THÊM, SỬA, XÓA TIỆN ÍCH CHO KHÁCH SẠN ================

        // 
        [HttpGet("get-all-amenity-types")]
        public async Task<IActionResult> GetAllAmenityTypeAsync()
        {
            var response = await _amenityService.GetTypeDataAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPost("create-amenity")]
        public async Task<IActionResult> CreateAmenityAsync([FromBody] AmenityCreateOrUpdateDTO newAmenity)
        {
            var response = await _amenityService.CreateAsync(newAmenity);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPut("update-amenity/{id}")]
        public async Task<IActionResult> UpdateAmenityAsync(int id, [FromBody] AmenityCreateOrUpdateDTO amenity)
        {

            var response = await _amenityService.UpdateAsync(id, amenity);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpDelete("delete-amenity/{id}")]
        public async Task<IActionResult> DeleteAmenityAsync(int id)
        {
            var response = await _amenityService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        #endregion

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


        #region MANAGE POLICY
        // ================= THÊM, SỬA, XÓA CHÍNH SÁCH CHO KHÁCH SẠN ================
        [HttpGet("get-all-policy-types")]
        public async Task<IActionResult> GetPolicyTypesAsync()
        {
            var response = await _policyService.GetTypeDataAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPost("create-policy")]
        public async Task<IActionResult> CreatePolicyAsync([FromBody] PolicyCreateOrUpdateDTO newPolicy)
        {
            var response = await _policyService.CreateAsync(newPolicy);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPut("update-policy/{id}")]
        public async Task<IActionResult> UpdatePolicyAsync(int id, [FromBody] PolicyCreateOrUpdateDTO policy)
        {

            var response = await _policyService.UpdateAsync(id, policy);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        // [Authorize(Roles = "Admin")]
        [HttpDelete("delete-policy/{id}")]
        public async Task<IActionResult> DeletePolicyAsync(int id)
        {
            var response = await _policyService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }
        #endregion

        #region MANAGE SERVICE 
        [HttpGet("get-all-service-types")]
        public async Task<IActionResult> GetAllServiceTypesAsync()
        {
            var response = await _hotelService.GetAllServiceTypesAsync();
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPost("create-standard-service")]
        public async Task<IActionResult> CreateStandardServiceAsync([FromBody] StdServiceCreateOrUpdateDTO newService)
        {
            var response = await _serviceManage.CreateAsync(newService);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPost("create-airport-transfer-service")]
        public async Task<IActionResult> CreateAirportTransferServiceAsync([FromBody] AirportTransServiceCreateOrUpdateDTO newService)
        {
            var response = await _serviceManage.CreateAsync(newService);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPut("update-standard-service/{id}")]
        public async Task<IActionResult> UpdateStandardServiceAsync(int id, [FromBody] StdServiceCreateOrUpdateDTO updatedService)
        {
            var response = await _serviceManage.UpdateAsync(id, updatedService);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpPut("update-airport-transfer-service/{id}")]
        public async Task<IActionResult> UpdateAirportTransferServiceAsync(int id, [FromBody] AirportTransServiceCreateOrUpdateDTO updatedService)
        {
            var response = await _serviceManage.UpdateAsync(id, updatedService);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        [HttpDelete("delete-service/{id}")]
        public async Task<IActionResult> DeleteServiceAsync(int id)
        {
            var response = await _serviceManage.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(response);
        }

        #endregion

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
