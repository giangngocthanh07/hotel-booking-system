using HotelBooking.webapp.ViewModels.Hotel;

public interface IManagementService
{
    // ==========================================
    // 1. GET DATA
    // ==========================================

    // 1. GET MENU
    Task<ApiResponse<ManageMenuResultVM>> GetManageModuleTypesOnly(ManageModuleEnum module);

    // 2. GET DATA (TYPED)
    Task<ApiResponse<PagedManageResult<ServiceBaseVM>>> GetServicesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<PolicyVM>>> GetPoliciesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<AmenityVM>>> GetAmenitiesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<RoomQualityVM>>> GetRoomQualitiesByType(int? typeId, PagingRequest paging);

    // 3. GET DATA (NON-TYPED)
    Task<ApiResponse<PagedManageResult<UnitTypeVM>>> GetUnitTypes(PagingRequest paging);
    Task<ApiResponse<PagedManageResult<BedTypeVM>>> GetBedTypes(PagingRequest paging);
    Task<ApiResponse<PagedManageResult<RoomViewVM>>> GetRoomViews(PagingRequest paging);


    // ==========================================
    // 2. DELETE (Xóa) - Trả về bool
    // ==========================================
    Task<ApiResponse<bool>> DeletePolicy(int id);
    Task<ApiResponse<bool>> DeleteAmenity(int id);
    Task<ApiResponse<bool>> DeleteRoomQuality(int id);
    Task<ApiResponse<bool>> DeleteService(int id);
    Task<ApiResponse<bool>> DeleteUnitType(int id);
    Task<ApiResponse<bool>> DeleteBedType(int id);
    Task<ApiResponse<bool>> DeleteRoomView(int id);

    // ==========================================
    // 3. CREATE (Tạo mới) - Trả về chính Object đó
    // ==========================================
    // ServiceBaseVM dùng logic riêng
    Task<ApiResponse<ServiceBaseVM>> CreateService(ServiceBaseVM vm);

    // Các loại khác dùng Generic
    Task<ApiResponse<PolicyVM>> CreatePolicy(PolicyVM vm);
    Task<ApiResponse<AmenityVM>> CreateAmenity(AmenityVM vm);
    Task<ApiResponse<RoomQualityVM>> CreateRoomQuality(RoomQualityVM vm);
    Task<ApiResponse<UnitTypeVM>> CreateUnitType(UnitTypeVM vm);
    Task<ApiResponse<BedTypeVM>> CreateBedType(BedTypeVM vm);
    Task<ApiResponse<RoomViewVM>> CreateRoomView(RoomViewVM vm);

    // ==========================================
    // 4. UPDATE (Cập nhật) - Trả về chính Object đó
    // ==========================================
    // ServiceBaseVM dùng logic riêng
    Task<ApiResponse<ServiceBaseVM>> UpdateService(ServiceBaseVM vm);

    // Các loại khác dùng Generic
    Task<ApiResponse<PolicyVM>> UpdatePolicy(PolicyVM vm);
    Task<ApiResponse<AmenityVM>> UpdateAmenity(AmenityVM vm);
    Task<ApiResponse<RoomQualityVM>> UpdateRoomQuality(RoomQualityVM vm);
    Task<ApiResponse<UnitTypeVM>> UpdateUnitType(UnitTypeVM vm);
    Task<ApiResponse<BedTypeVM>> UpdateBedType(BedTypeVM vm);
    Task<ApiResponse<RoomViewVM>> UpdateRoomView(RoomViewVM vm);

}

public class ManagementService : IManagementService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _http;

    public ManagementService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _http = _httpClientFactory.CreateClient("HotelBookingAPI");
    }

    // ==========================================
    // 1. GỌI API LẤY MENU (SIÊU NHẸ)
    // ==========================================
    public async Task<ApiResponse<ManageMenuResultVM>> GetManageModuleTypesOnly(ManageModuleEnum module)
    {
        try
        {
            // Backend Route: [HttpGet("manage/menu/{module}")]
            // VD: /api/hotel/manage/menu/Service
            var url = $"hotel/get-manage-menu/{module}";

            var response = await _http.GetFromJsonAsync<ApiResponse<ManageMenuResultVM>>(url);
            return response!;
        }
        catch (Exception)
        {
            // Xử lý lỗi kết nối (ví dụ server tắt)
            return ResponseFactory.ServerError<ManageMenuResultVM>();
        }
    }

    // ==========================================
    // 1. GET: 
    // ==========================================
    // Typed Groups
    public Task<ApiResponse<PagedManageResult<ServiceBaseVM>>> GetServicesByType(int? typeId, PagingRequest paging) => GetGenericTyped<ServiceBaseVM>("hotel/get-service-data", typeId, paging);
    public Task<ApiResponse<PagedManageResult<PolicyVM>>> GetPoliciesByType(int? typeId, PagingRequest paging) => GetGenericTyped<PolicyVM>("hotel/get-policy-data", typeId, paging);
    public Task<ApiResponse<PagedManageResult<AmenityVM>>> GetAmenitiesByType(int? typeId, PagingRequest paging) => GetGenericTyped<AmenityVM>("hotel/get-amenity-data", typeId, paging);
    public Task<ApiResponse<PagedManageResult<RoomQualityVM>>> GetRoomQualitiesByType(int? typeId, PagingRequest paging) => GetGenericTyped<RoomQualityVM>("hotel/get-room-quality-data", typeId, paging);

    // Non-Typed Groups
    public Task<ApiResponse<PagedManageResult<UnitTypeVM>>> GetUnitTypes(PagingRequest paging)
        => GetAttributePaged<UnitTypeVM>(RoomAttributeType.UnitType, paging);
    public Task<ApiResponse<PagedManageResult<BedTypeVM>>> GetBedTypes(PagingRequest paging)
        => GetAttributePaged<BedTypeVM>(RoomAttributeType.BedType, paging);
    public Task<ApiResponse<PagedManageResult<RoomViewVM>>> GetRoomViews(PagingRequest paging)
        => GetAttributePaged<RoomViewVM>(RoomAttributeType.RoomView, paging);

    // ==========================================
    // 2. DELETE: Xóa dữ liệu
    // ==========================================
    public Task<ApiResponse<bool>> DeleteService(int id) => DeleteGeneric("delete-service", id);
    public Task<ApiResponse<bool>> DeletePolicy(int id) => DeleteGeneric("delete-policy", id);
    public Task<ApiResponse<bool>> DeleteAmenity(int id) => DeleteGeneric("delete-amenity", id);
    public Task<ApiResponse<bool>> DeleteRoomQuality(int id) => DeleteGeneric("delete-room-quality", id);
    public Task<ApiResponse<bool>> DeleteRoomView(int id) => DeleteGeneric("delete-room-view", id);
    public Task<ApiResponse<bool>> DeleteBedType(int id) => DeleteGeneric("delete-bed-type", id);
    public Task<ApiResponse<bool>> DeleteUnitType(int id) => DeleteGeneric("delete-unit-type", id);


    // ==========================================
    // 3. CREATE: Tạo mới
    // ==========================================
    // Lưu ý: Helper của bạn yêu cầu Input và Output cùng kiểu T

    // [ĐẶC BIỆT] ServiceBaseVM có logic URL phức tạp (Đa hình) -> Không dùng Generic Helper chung được
    public async Task<ApiResponse<ServiceBaseVM>> CreateService(ServiceBaseVM vm)
    {
        try
        {
            string url = GetServiceUrl(vm, isUpdate: false);
            // Tự động hiểu T là ServiceBaseDTO
            return await _http.PostApiAsync(url, vm);
        }
        catch (Exception ex)
        {
            return ResponseFactory.Failure<ServiceBaseVM>(StatusCodeResponse.Error, ex.Message);
        }
    }

    // [CƠ BẢN] Các hàm Create đơn giản -> Dùng PostGeneric cho gọn
    // [CƠ BẢN] Các hàm Create đơn giản -> Dùng PostGeneric cho gọn
    public Task<ApiResponse<PolicyVM>> CreatePolicy(PolicyVM vm) => PostGeneric("create-policy", vm);
    public Task<ApiResponse<AmenityVM>> CreateAmenity(AmenityVM vm) => PostGeneric("create-amenity", vm);
    public Task<ApiResponse<RoomQualityVM>> CreateRoomQuality(RoomQualityVM vm) => PostGeneric("create-room-quality", vm);
    public Task<ApiResponse<UnitTypeVM>> CreateUnitType(UnitTypeVM vm) => PostGeneric("create-unit-type", vm);
    public Task<ApiResponse<BedTypeVM>> CreateBedType(BedTypeVM vm) => PostGeneric("create-bed-type", vm);
    public Task<ApiResponse<RoomViewVM>> CreateRoomView(RoomViewVM vm) => PostGeneric("create-room-view", vm);

    // ==========================================
    // 4. UPDATE: Cập nhật
    // ==========================================
    // [ĐẶC BIỆT] ServiceBaseVM có logic URL phức tạp -> Giữ nguyên logic riêng
    public async Task<ApiResponse<ServiceBaseVM>> UpdateService(ServiceBaseVM vm)
    {
        try
        {
            string url = GetServiceUrl(vm, isUpdate: true);
            return await _http.PutApiAsync<ServiceBaseVM>(url, vm);
        }
        catch (Exception ex)
        {
            return ResponseFactory.Failure<ServiceBaseVM>(StatusCodeResponse.Error, ex.Message);
        }
    }

    // [CƠ BẢN] Các hàm Update đơn giản -> Dùng PutGeneric cho gọn
    public Task<ApiResponse<PolicyVM>> UpdatePolicy(PolicyVM vm) => PutGeneric("update-policy", vm.Id, vm);
    public Task<ApiResponse<AmenityVM>> UpdateAmenity(AmenityVM vm) => PutGeneric("update-amenity", vm.Id, vm);
    public Task<ApiResponse<RoomQualityVM>> UpdateRoomQuality(RoomQualityVM vm) => PutGeneric("update-room-quality", vm.Id, vm);
    public Task<ApiResponse<UnitTypeVM>> UpdateUnitType(UnitTypeVM vm) => PutGeneric("update-unit-type", vm.Id, vm);
    public Task<ApiResponse<BedTypeVM>> UpdateBedType(BedTypeVM vm) => PutGeneric("update-bed-type", vm.Id, vm);
    public Task<ApiResponse<RoomViewVM>> UpdateRoomView(RoomViewVM vm) => PutGeneric("update-room-view", vm.Id, vm);


    // --- Helper chọn URL riêng trong Service này ---
    private string GetServiceUrl(ServiceBaseVM vm, bool isUpdate)
    {
        // 1. Xác định "Slug" (tên định danh) của loại dịch vụ
        string typeSlug = vm switch
        {
            ServiceStandardVM => "standard-service",
            ServiceAirportTransferVM => "airport-transfer-service",
            _ => throw new NotSupportedException($"Chưa cấu hình cho: {vm.GetType().Name}")
        };

        // 2. Ghép chuỗi tự động
        if (isUpdate)
        {
            return $"hotel/update-{typeSlug}/{vm.Id}";
        }
        else
        {
            return $"hotel/create-{typeSlug}";
        }
    }

    // Helper 1: Gọi API Phân trang (Get Paged Data)
    private async Task<ApiResponse<PagedManageResult<T>>> GetAttributePaged<T>(RoomAttributeType type, PagingRequest paging, int? typeId = null)
    {
        // Build URL: api/hotel/room-attribute/get-paged-data?type=UnitType&pageIndex=1...
        var url = $"hotel/room-attribute/get-paged-data?type={type}&pageIndex={paging.PageIndex}&pageSize={paging.PageSize}";
        
        if (typeId.HasValue) url += $"&typeId={typeId}";

        return await _http.GetApiAsync<PagedManageResult<T>>(url);
    }

    // ==========================================
    // 5. PRIVATE GENERIC HELPERS (Nồi dùng chung)
    // ==========================================
    private Task<ApiResponse<PagedManageResult<T>>> GetGenericTyped<T>(string endpoint, int? typeId, PagingRequest paging)
    {
        // Xây dựng Query String
        // Bắt đầu với endpoint
        var url = endpoint;
        var queryParams = new List<string>();

        // Thêm typeId nếu có
        if (typeId.HasValue)
        {
            queryParams.Add($"typeId={typeId}");
        }

        // Thêm tham số phân trang nếu có
        if (paging != null)
        {
             // Sử dụng giá trị mặc định nếu null (tuy nhiên logic validate ở BE đã xử lý, ở đây cứ truyền lên)
             // Lưu ý: Nếu paging.PageIndex là null, string interpolation sẽ ra chuỗi rỗng hoặc lỗi tùy cấu hình,
             // nên tốt nhất check null hoặc dùng .GetValueOrDefault().
             // Giả sử PagingRequest ở Client cũng dùng int? như Server
             if(paging.PageIndex.HasValue) queryParams.Add($"pageIndex={paging.PageIndex}");
             if(paging.PageSize.HasValue) queryParams.Add($"pageSize={paging.PageSize}");
        }

        // Nối query string vào URL
        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        return _http.GetApiAsync<PagedManageResult<T>>(url);
    }
    // Helper cho CREATE
    private Task<ApiResponse<T>> PostGeneric<T>(string endpoint, T vm)
    {
        // URL: hotel/create-policy
        return _http.PostApiAsync<T>($"hotel/{endpoint}", vm);
    }

    // Helper cho UPDATE
    private Task<ApiResponse<T>> PutGeneric<T>(string endpoint, int id, T vm)
    {
        // URL: hotel/update-policy/123
        return _http.PutApiAsync<T>($"hotel/{endpoint}/{id}", vm);
    }

    // Helper cho DELETE
    private async Task<ApiResponse<bool>> DeleteGeneric(string endpoint, int id)
    {
        return await _http.DeleteApiAsync<bool>($"hotel/{endpoint}/{id}");
    }
}