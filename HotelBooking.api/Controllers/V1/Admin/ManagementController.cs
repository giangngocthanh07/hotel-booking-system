using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.AdminManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBooking.api.Controllers.V1.Admin
{
    /// <summary>
    /// Admin Management Controller - Quản lý các module (Amenity, Policy, Service)
    /// </summary>
    [Route("api/v1/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ManagementController : ControllerBase
    {
        private readonly IManagementAdminService _managementAdminService;
        private readonly IAmenityService _amenityService;
        private readonly IPolicyService _policyService;
        private readonly IServiceService _serviceService;
        private readonly IRoomQualityService _rqService;
        private readonly IRoomAttributeFacade _roomAttributeFacade;

        public ManagementController(
            IManagementAdminService managementAdminService,
            IAmenityService amenityService,
            IPolicyService policyService,
            IServiceService serviceService,
            IRoomQualityService rqService,
            IRoomAttributeFacade roomAttributeFacade)
        {
            _managementAdminService = managementAdminService;
            _amenityService = amenityService;
            _policyService = policyService;
            _serviceService = serviceService;
            _rqService = rqService;
            _roomAttributeFacade = roomAttributeFacade;

        }

        /// <summary>
        /// Lấy cấu trúc Menu (Modules + Types)
        /// </summary>
        /// 
        // ==========================================
        // 1. API LẤY MENU (SIÊU NHẸ)
        // ==========================================
        // URL: api/hotel/manage/menu/Service
        // Nhiệm vụ: Chỉ trả về danh sách Types để Frontend vẽ Dropdown
        [HttpGet("get-manage-menu/{module}")]
        public async Task<IActionResult> GetManageMenu(ManageModuleEnum module)
        {
            var result = await _managementAdminService.GetManageMenuAsync(module);
            return Ok(result);
        }

        // ==========================================
        // 2. API LẤY DỮ LIỆU (THEO TỪNG MODULE)
        // ==========================================
        // Tại sao không gom 1 cái generic? -> Để Swagger hiển thị rõ DTO trả về cho từng loại

        #region AMENITY MANAGEMENT

        /// <summary>
        /// Lấy danh sách các loại amenity
        /// </summary>
        /// 
        [HttpGet("get-all-amenity-types")]
        public async Task<IActionResult> GetAmenityTypes()
        {
            var result = await _amenityService.GetTypeDataAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Lấy danh sách amenity theo loại (có phân trang)
        /// </summary>
        /// 
        // URL: api/hotel/get-amenity-data?typeId=1
        [HttpGet("get-amenity-data")]
        public async Task<IActionResult> GetAmenitiesByType(
            [FromQuery] int? typeId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var paging = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };
            var result = await _amenityService.GetAmenitiesByTypeAsync(typeId, paging);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Thêm amenity mới
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpPost("create-amenity")]
        public async Task<IActionResult> CreateAmenity([FromBody] AmenityCreateOrUpdateDTO dto)
        {
            var result = await _amenityService.CreateAsync(dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Cập nhật amenity
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpPut("update-amenity/{id}")]
        public async Task<IActionResult> UpdateAmenity(int id, [FromBody] AmenityCreateOrUpdateDTO dto)
        {
            var result = await _amenityService.UpdateAsync(id, dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Xóa amenity
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-amenity/{id}")]
        public async Task<IActionResult> DeleteAmenity(int id)
        {
            var result = await _amenityService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        #endregion

        #region POLICY MANAGEMENT

        /// <summary>
        /// Lấy danh sách các loại policy
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-policy-types")]
        public async Task<IActionResult> GetPolicyTypes()
        {
            var result = await _policyService.GetTypeDataAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Lấy danh sách policy theo loại (có phân trang)
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpGet("get-policy-data")]
        public async Task<IActionResult> GetPoliciesByType(
            [FromQuery] int? typeId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var paging = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };
            var result = await _policyService.GetPoliciesByTypeAsync(typeId, paging);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Thêm policy mới
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpPost("create-policy")]
        public async Task<IActionResult> CreatePolicy([FromBody] PolicyCreateOrUpdateDTO dto)
        {
            var result = await _policyService.CreateAsync(dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Cập nhật policy
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpPut("update-policy/{id}")]
        public async Task<IActionResult> UpdatePolicy(int id, [FromBody] PolicyCreateOrUpdateDTO dto)
        {
            var result = await _policyService.UpdateAsync(id, dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Xóa policy
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-policy/{id}")]
        public async Task<IActionResult> DeletePolicy(int id)
        {
            var result = await _policyService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        #endregion

        #region SERVICE MANAGEMENT

        /// <summary>
        /// Lấy danh sách các loại service
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpGet("get-all-service-types")]
        public async Task<IActionResult> GetServiceTypes()
        {
            var result = await _serviceService.GetTypeDataAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Lấy danh sách service theo loại (có phân trang)
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        // URL: api/hotel/get-service-data?typeId=1
        [HttpGet("get-service-data")]
        public async Task<IActionResult> GetServicesByType(
            [FromQuery] int? typeId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var paging = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };
            var result = await _serviceService.GetServicesByTypeAsync(typeId, paging);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Thêm service mới
        /// </summary>

        [Authorize(Roles = "Admin")]
        [HttpPost("create-standard-service")]
        public async Task<IActionResult> CreateStandardServiceAsync([FromBody] StdServiceCreateOrUpdateDTO newService)
        {
            var result = await _serviceService.CreateAsync(newService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-airport-transfer-service")]
        public async Task<IActionResult> CreateAirportTransferServiceAsync([FromBody] AirportTransServiceCreateOrUpdateDTO newService)
        {
            var result = await _serviceService.CreateAsync(newService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Cập nhật service
        /// </summary>

        [Authorize(Roles = "Admin")]
        [HttpPut("update-standard-service/{id}")]
        public async Task<IActionResult> UpdateStandardServiceAsync(int id, [FromBody] StdServiceCreateOrUpdateDTO updatedService)
        {
            var result = await _serviceService.UpdateAsync(id, updatedService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-airport-transfer-service/{id}")]
        public async Task<IActionResult> UpdateAirportTransferServiceAsync(int id, [FromBody] AirportTransServiceCreateOrUpdateDTO updatedService)
        {
            var result = await _serviceService.UpdateAsync(id, updatedService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Xóa service
        /// </summary>
        /// 
        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-service/{id}")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var result = await _serviceService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        #endregion

        #region MANAGE ROOM QUALITY
        [HttpGet("get-room-quality-data")]
        public async Task<IActionResult> GetRoomQualityData([FromQuery] int? typeId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var request = new PagingRequest { PageIndex = pageIndex, PageSize = pageSize };

            var result = await _rqService.GetRoomQualitiesByTypeAsync(typeId, request);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-room-quality")]
        public async Task<IActionResult> CreateRoomQualityAsync([FromBody] RoomQualityCreateOrUpdateDTO newRoomQuality)
        {
            var result = await _rqService.CreateAsync(newRoomQuality);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-room-quality/{id}")]
        public async Task<IActionResult> UpdateRoomQualityAsync(int id, [FromBody] RoomQualityCreateOrUpdateDTO rq)
        {

            var result = await _rqService.UpdateAsync(id, rq);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpDelete("delete-room-quality/{id}")]
        public async Task<IActionResult> DeleteRoomQualityAsync(int id)
        {
            var result = await _rqService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
        #endregion

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
            var result = await _roomAttributeFacade.GetPagedByTypeAsync(type, paging, typeId);

            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        #region MANAGE UNIT TYPE

        [Authorize(Roles = "Admin")]
        [HttpGet("get-unit-type-data")]
        public async Task<IActionResult> GetUnitTypeData()
        {
            var result = await _roomAttributeFacade.UnitTypeService.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-unit-type")]
        public async Task<IActionResult> CreateUnitTypeAsync([FromBody] UnitTypeCreateOrUpdateDTO newUnitType)
        {
            var result = await _roomAttributeFacade.UnitTypeService.CreateAsync(newUnitType);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-unit-type/{id}")]
        public async Task<IActionResult> UpdateUnitTypeAsync(int id, [FromBody] UnitTypeCreateOrUpdateDTO ut)
        {

            var result = await _roomAttributeFacade.UnitTypeService.UpdateAsync(id, ut);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-unit-type/{id}")]
        public async Task<IActionResult> DeleteUnitTypeAsync(int id)
        {
            var result = await _roomAttributeFacade.UnitTypeService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
        #endregion

        #region MANAGE BED TYPE

        [Authorize(Roles = "Admin")]
        [HttpGet("get-bed-type-data")]
        public async Task<IActionResult> GetBedTypeData()
        {
            var result = await _roomAttributeFacade.BedTypeService.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-bed-type")]
        public async Task<IActionResult> CreateBedTypeAsync([FromBody] BedTypeCreateOrUpdateDTO newBedType)
        {
            var result = await _roomAttributeFacade.BedTypeService.CreateAsync(newBedType);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-bed-type/{id}")]
        public async Task<IActionResult> UpdateBedTypeAsync(int id, [FromBody] BedTypeCreateOrUpdateDTO bt)
        {

            var result = await _roomAttributeFacade.BedTypeService.UpdateAsync(id, bt);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-bed-type/{id}")]
        public async Task<IActionResult> DeleteBedTypeAsync(int id)
        {
            var result = await _roomAttributeFacade.BedTypeService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        #endregion

        #region MANAGE ROOM VIEW

        [Authorize(Roles = "Admin")]
        [HttpGet("get-room-view-data")]
        public async Task<IActionResult> GetRoomView()
        {
            var result = await _roomAttributeFacade.RoomViewService.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create-room-view")]
        public async Task<IActionResult> CreateRoomViewAsync([FromBody] RoomViewCreateOrUpdateDTO newRoomView)
        {
            var result = await _roomAttributeFacade.RoomViewService.CreateAsync(newRoomView);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update-room-view/{id}")]
        public async Task<IActionResult> UpdateRoomViewAsync(int id, [FromBody] RoomViewCreateOrUpdateDTO rv)
        {

            var result = await _roomAttributeFacade.RoomViewService.UpdateAsync(id, rv);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-room-view/{id}")]
        public async Task<IActionResult> DeleteRoomViewAsync(int id)
        {
            var result = await _roomAttributeFacade.RoomViewService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
        #endregion
    }


}
