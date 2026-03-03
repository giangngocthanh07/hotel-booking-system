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
    [Tags("Admin - Management")]
    public class ManagementController : ControllerBase
    {
        private readonly IManagementAdminService _managementAdminService;
        private readonly IAmenityService _amenityService;
        private readonly IPolicyService _policyService;
        private readonly IServiceService _serviceService;
        private readonly IRoomQualityService _roomQualityService;
        private readonly IRoomAttributeFacade _roomAttributeFacade;

        public ManagementController(
            IManagementAdminService managementAdminService,
            IAmenityService amenityService,
            IPolicyService policyService,
            IServiceService serviceService,
            IRoomQualityService roomQualityService,
            IRoomAttributeFacade roomAttributeFacade)
        {
            _managementAdminService = managementAdminService;
            _amenityService = amenityService;
            _policyService = policyService;
            _serviceService = serviceService;
            _roomQualityService = roomQualityService;
            _roomAttributeFacade = roomAttributeFacade;

        }

        /// <summary>
        /// Retrieve menu structure (Modules + Types)
        /// </summary>
        /// 
        // ==========================================
        // 1. GET MENU API (lightweight)
        // ==========================================
        // URL: api/hotel/manage/menu/Service
        // Purpose: Returns the list of Types for the Frontend to render a Dropdown
        [HttpGet("get-manage-menu/{module}")]
        public async Task<IActionResult> GetManageMenu(ManageModuleEnum module)
        {
            // Wrap parameters into a Request Object to pass down to Service (so FluentValidation runs inside Service)
            var request = new ManageMenuRequest
            {
                Module = module
            };
            var result = await _managementAdminService.GetManageMenuAsync(request);
            return ApiResponseHandlerHelper.HandleResponse(result);
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
        [HttpPost("create-amenity")]
        public async Task<IActionResult> CreateAmenity([FromBody] AmenityCreateDTO dto)
        {
            var result = await _amenityService.CreateAsync(dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Cập nhật amenity
        /// </summary>
        /// 
        [HttpPut("update-amenity/{id}")]
        public async Task<IActionResult> UpdateAmenity(int id, [FromBody] AmenityUpdateDTO dto)
        {
            var result = await _amenityService.UpdateAsync(id, dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Xóa amenity
        /// </summary>
        /// 
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
        [HttpPost("create-check-in-out-policy")]
        public async Task<IActionResult> CreateCheckInCheckOutPolicy([FromBody] CheckInOutPolicyCreateDTO dto)
        {
            var result = await _policyService.CreateAsync(dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-cancellation-policy")]
        public async Task<IActionResult> CreateCancellationPolicy([FromBody] CancellationPolicyCreateDTO dto)
        {
            var result = await _policyService.CreateAsync(dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-pet-policy")]
        public async Task<IActionResult> CreatePetPolicy([FromBody] PetPolicyCreateDTO dto)
        {
            var result = await _policyService.CreateAsync(dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-children-policy")]
        public async Task<IActionResult> CreateChildrenPolicy([FromBody] ChildrenPolicyCreateDTO dto)
        {
            var result = await _policyService.CreateAsync(dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Cập nhật policy
        /// </summary>
        /// 
        [HttpPut("update-check-in-out-policy/{id}")]
        public async Task<IActionResult> UpdateCheckInOutPolicy(int id, [FromBody] CheckInOutPolicyUpdateDTO dto)
        {
            var result = await _policyService.UpdateAsync(id, dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-cancellation-policy/{id}")]
        public async Task<IActionResult> UpdateCancellationPolicy(int id, [FromBody] CancellationPolicyUpdateDTO dto)
        {
            var result = await _policyService.UpdateAsync(id, dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-pet-policy/{id}")]
        public async Task<IActionResult> UpdatePetPolicy(int id, [FromBody] PetPolicyUpdateDTO dto)
        {
            var result = await _policyService.UpdateAsync(id, dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-children-policy/{id}")]
        public async Task<IActionResult> UpdateChildrenPolicy(int id, [FromBody] ChildrenPolicyUpdateDTO dto)
        {
            var result = await _policyService.UpdateAsync(id, dto);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Xóa policy
        /// </summary>
        /// 
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
        [HttpPost("create-standard-service")]
        public async Task<IActionResult> CreateStandardServiceAsync([FromBody] ServiceStandardCreateDTO newService)
        {
            var result = await _serviceService.CreateAsync(newService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-airport-transfer-service")]
        public async Task<IActionResult> CreateAirportTransferServiceAsync([FromBody] ServiceAirportCreateDTO newService)
        {
            var result = await _serviceService.CreateAsync(newService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Cập nhật service
        /// </summary>

        [HttpPut("update-standard-service/{id}")]
        public async Task<IActionResult> UpdateStandardServiceAsync(int id, [FromBody] ServiceStandardUpdateDTO updatedService)
        {
            var result = await _serviceService.UpdateAsync(id, updatedService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-airport-transfer-service/{id}")]
        public async Task<IActionResult> UpdateAirportTransferServiceAsync(int id, [FromBody] ServiceAirportUpdateDTO updatedService)
        {
            var result = await _serviceService.UpdateAsync(id, updatedService);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        /// <summary>
        /// Xóa service
        /// </summary>
        /// 
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

            var result = await _roomQualityService.GetRoomQualitiesByTypeAsync(typeId, request);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-room-quality")]
        public async Task<IActionResult> CreateRoomQualityAsync([FromBody] RoomQualityCreateDTO newRoomQuality)
        {
            var result = await _roomQualityService.CreateAsync(newRoomQuality);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-room-quality/{id}")]
        public async Task<IActionResult> UpdateRoomQualityAsync(int id, [FromBody] RoomQualityUpdateDTO rq)
        {

            var result = await _roomQualityService.UpdateAsync(id, rq);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpDelete("delete-room-quality/{id}")]
        public async Task<IActionResult> DeleteRoomQualityAsync(int id)
        {
            var result = await _roomQualityService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
        #endregion

        [HttpGet("room-attribute/get-paged-data")]
        public async Task<IActionResult> GetPagedData(
            [FromQuery] RoomAttributeType type, // Enum: UnitType, BedType, RoomView...
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? typeId = null)     // Only used for RoomQuality
        {
            // 1. Build PagingRequest from individual parameters
            var pagingRequest = new PagingRequest
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            // 2. Wrap into Request Object (preparing for FluentValidation inside)
            var request = new GetRoomAttributeRequest
            {
                Type = type,
                Paging = pagingRequest, // Assign the object created above
                TypeId = typeId
            };

            // 3. Call Facade (NOTE: pass the correct 'request' variable)
            var result = await _roomAttributeFacade.GetPagedByTypeAsync(request);

            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        #region MANAGE UNIT TYPE

        [HttpGet("get-unit-type-data")]
        public async Task<IActionResult> GetUnitTypeData()
        {
            var result = await _roomAttributeFacade.UnitTypeService.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-unit-type")]
        public async Task<IActionResult> CreateUnitTypeAsync([FromBody] UnitTypeCreateDTO newUnitType)
        {
            var result = await _roomAttributeFacade.UnitTypeService.CreateAsync(newUnitType);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-unit-type/{id}")]
        public async Task<IActionResult> UpdateUnitTypeAsync(int id, [FromBody] UnitTypeUpdateDTO ut)
        {

            var result = await _roomAttributeFacade.UnitTypeService.UpdateAsync(id, ut);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpDelete("delete-unit-type/{id}")]
        public async Task<IActionResult> DeleteUnitTypeAsync(int id)
        {
            var result = await _roomAttributeFacade.UnitTypeService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
        #endregion

        #region MANAGE BED TYPE

        [HttpGet("get-bed-type-data")]
        public async Task<IActionResult> GetBedTypeData()
        {
            var result = await _roomAttributeFacade.BedTypeService.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-bed-type")]
        public async Task<IActionResult> CreateBedTypeAsync([FromBody] BedTypeCreateDTO newBedType)
        {
            var result = await _roomAttributeFacade.BedTypeService.CreateAsync(newBedType);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-bed-type/{id}")]
        public async Task<IActionResult> UpdateBedTypeAsync(int id, [FromBody] BedTypeUpdateDTO bt)
        {

            var result = await _roomAttributeFacade.BedTypeService.UpdateAsync(id, bt);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpDelete("delete-bed-type/{id}")]
        public async Task<IActionResult> DeleteBedTypeAsync(int id)
        {
            var result = await _roomAttributeFacade.BedTypeService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        #endregion

        #region MANAGE ROOM VIEW

        [HttpGet("get-room-view-data")]
        public async Task<IActionResult> GetRoomView()
        {
            var result = await _roomAttributeFacade.RoomViewService.GetAllAsync();
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPost("create-room-view")]
        public async Task<IActionResult> CreateRoomViewAsync([FromBody] RoomViewCreateDTO newRoomView)
        {
            var result = await _roomAttributeFacade.RoomViewService.CreateAsync(newRoomView);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpPut("update-room-view/{id}")]
        public async Task<IActionResult> UpdateRoomViewAsync(int id, [FromBody] RoomViewUpdateDTO rv)
        {

            var result = await _roomAttributeFacade.RoomViewService.UpdateAsync(id, rv);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }

        [HttpDelete("delete-room-view/{id}")]
        public async Task<IActionResult> DeleteRoomViewAsync(int id)
        {
            var result = await _roomAttributeFacade.RoomViewService.DeleteAsync(id);
            return ApiResponseHandlerHelper.HandleResponse(result);
        }
        #endregion
    }


}
