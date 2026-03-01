using HotelBooking.webapp.Services.Interface;

/// <summary>
/// Interface cho Management Service - quản lý các entities (Amenity, BedType, etc.)
/// Kế thừa ITokenService để hỗ trợ AdminPageBase&lt;TService&gt; generic.
/// </summary>
public interface IManagementService : ITokenService
{

    // 1. GET MENU
    Task<ApiResponse<ManageMenuResultVM>> GetManageModuleTypesOnly(ManageModuleEnum module);

    // 2. GET DATA LIST (PAGING)
    // --- Nhóm Typed (Có TypeId) ---
    Task<ApiResponse<PagedManageResult<ServiceVM>>> GetServicesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<PolicyVM>>> GetPoliciesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<AmenityVM>>> GetAmenitiesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<RoomQualityVM>>> GetRoomQualitiesByType(int? typeId, PagingRequest paging);

    // --- Nhóm Attributes (Không TypeId - Đã tách API riêng) ---
    Task<ApiResponse<PagedManageResult<UnitTypeVM>>> GetUnitTypes(PagingRequest paging);
    Task<ApiResponse<PagedManageResult<BedTypeVM>>> GetBedTypes(PagingRequest paging);
    Task<ApiResponse<PagedManageResult<RoomViewVM>>> GetRoomViews(PagingRequest paging);

    // 3. DELETE
    Task<ApiResponse<bool>> DeleteService(int id);
    Task<ApiResponse<bool>> DeletePolicy(int id);
    Task<ApiResponse<bool>> DeleteAmenity(int id);
    Task<ApiResponse<bool>> DeleteRoomQuality(int id);
    Task<ApiResponse<bool>> DeleteUnitType(int id);
    Task<ApiResponse<bool>> DeleteBedType(int id);
    Task<ApiResponse<bool>> DeleteRoomView(int id);

    // 4. CREATE (Nhận CreateVM, Trả về OutputVM)
    Task<ApiResponse<ServiceVM>> CreateService(ServiceCreateVM vm);
    Task<ApiResponse<PolicyVM>> CreatePolicy(PolicyCreateVM vm);
    Task<ApiResponse<AmenityVM>> CreateAmenity(AmenityCreateVM vm);
    Task<ApiResponse<RoomQualityVM>> CreateRoomQuality(RoomQualityCreateVM vm);
    Task<ApiResponse<UnitTypeVM>> CreateUnitType(UnitTypeCreateVM vm);
    Task<ApiResponse<BedTypeVM>> CreateBedType(BedTypeCreateVM vm);
    Task<ApiResponse<RoomViewVM>> CreateRoomView(RoomViewCreateVM vm);

    // 5. UPDATE (Nhận ID và UpdateVM, Trả về OutputVM)
    Task<ApiResponse<ServiceVM>> UpdateService(int id, ServiceUpdateVM vm);
    Task<ApiResponse<PolicyVM>> UpdatePolicy(int id, PolicyUpdateVM vm);
    Task<ApiResponse<AmenityVM>> UpdateAmenity(int id, AmenityUpdateVM vm);
    Task<ApiResponse<RoomQualityVM>> UpdateRoomQuality(int id, RoomQualityUpdateVM vm);
    Task<ApiResponse<UnitTypeVM>> UpdateUnitType(int id, UnitTypeUpdateVM vm);
    Task<ApiResponse<BedTypeVM>> UpdateBedType(int id, BedTypeUpdateVM vm);
    Task<ApiResponse<RoomViewVM>> UpdateRoomView(int id, RoomViewUpdateVM vm);
}

public class ManagementService : IManagementService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _http;
    private const string BaseUrl = "v1/admin/Management";

    public ManagementService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _http = _httpClientFactory.CreateClient("HotelBookingAPI");
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    // ==========================================
    // 1. MENU & TYPED DATA
    // ==========================================
    public async Task<ApiResponse<ManageMenuResultVM>> GetManageModuleTypesOnly(ManageModuleEnum module)
    {
        // Gửi Request Object qua Query String (Nếu BE đã sửa dùng DTO cho Menu)
        // Nếu BE vẫn dùng enum path param: "get-manage-menu/{module}" -> Giữ nguyên
        // Nếu BE đổi sang DTO: "get-manage-menu?module=1" -> Cần sửa lại
        // Giả sử BE vẫn giữ nguyên Route cũ:
        return await _http.GetApiAsync<ManageMenuResultVM>($"v1/admin/Management/get-manage-menu/{module}");
    }

    public Task<ApiResponse<PagedManageResult<ServiceVM>>> GetServicesByType(int? typeId, PagingRequest paging)
        => GetGenericTyped<ServiceVM>("v1/admin/Management/get-service-data", typeId, paging);

    public Task<ApiResponse<PagedManageResult<PolicyVM>>> GetPoliciesByType(int? typeId, PagingRequest paging)
        => GetGenericTyped<PolicyVM>("v1/admin/Management/get-policy-data", typeId, paging);

    public Task<ApiResponse<PagedManageResult<AmenityVM>>> GetAmenitiesByType(int? typeId, PagingRequest paging)
        => GetGenericTyped<AmenityVM>("v1/admin/Management/get-amenity-data", typeId, paging);

    public Task<ApiResponse<PagedManageResult<RoomQualityVM>>> GetRoomQualitiesByType(int? typeId, PagingRequest paging)
        => GetGenericTyped<RoomQualityVM>("v1/admin/Management/get-room-quality-data", typeId, paging);

    // ==========================================
    // 2. NON-TYPED DATA (ROOM ATTRIBUTES)
    // ==========================================
    public Task<ApiResponse<PagedManageResult<UnitTypeVM>>> GetUnitTypes(PagingRequest paging)
        => GetAttributePaged<UnitTypeVM>(RoomAttributeType.UnitType, paging);

    public Task<ApiResponse<PagedManageResult<BedTypeVM>>> GetBedTypes(PagingRequest paging)
        => GetAttributePaged<BedTypeVM>(RoomAttributeType.BedType, paging);

    public Task<ApiResponse<PagedManageResult<RoomViewVM>>> GetRoomViews(PagingRequest paging)
        => GetAttributePaged<RoomViewVM>(RoomAttributeType.RoomView, paging);

    // ==========================================
    // 3. DELETE
    // ==========================================
    public Task<ApiResponse<bool>> DeleteService(int id) => DeleteGeneric("delete-service", id);
    public Task<ApiResponse<bool>> DeletePolicy(int id) => DeleteGeneric("delete-policy", id);
    public Task<ApiResponse<bool>> DeleteAmenity(int id) => DeleteGeneric("delete-amenity", id);
    public Task<ApiResponse<bool>> DeleteRoomQuality(int id) => DeleteGeneric("delete-room-quality", id);
    public Task<ApiResponse<bool>> DeleteRoomView(int id) => DeleteGeneric("delete-room-view", id);
    public Task<ApiResponse<bool>> DeleteBedType(int id) => DeleteGeneric("delete-bed-type", id);
    public Task<ApiResponse<bool>> DeleteUnitType(int id) => DeleteGeneric("delete-unit-type", id);

    // =========================================================================
    // 4. CREATE (Dùng CreateVM)
    // =========================================================================
    public Task<ApiResponse<ServiceVM>> CreateService(ServiceCreateVM vm)
        => PostGenericWithSlug<ServiceVM, ServiceCreateVM>(vm, isService: true);

    public Task<ApiResponse<PolicyVM>> CreatePolicy(PolicyCreateVM vm)
        => PostGeneric<PolicyVM, PolicyCreateVM>("create-policy", vm);

    public Task<ApiResponse<AmenityVM>> CreateAmenity(AmenityCreateVM vm)
        => PostGeneric<AmenityVM, AmenityCreateVM>("create-amenity", vm);

    public Task<ApiResponse<RoomQualityVM>> CreateRoomQuality(RoomQualityCreateVM vm)
        => PostGeneric<RoomQualityVM, RoomQualityCreateVM>("create-room-quality", vm);

    public Task<ApiResponse<UnitTypeVM>> CreateUnitType(UnitTypeCreateVM vm)
        => PostGeneric<UnitTypeVM, UnitTypeCreateVM>("create-unit-type", vm);

    public Task<ApiResponse<BedTypeVM>> CreateBedType(BedTypeCreateVM vm)
        => PostGeneric<BedTypeVM, BedTypeCreateVM>("create-bed-type", vm);

    public Task<ApiResponse<RoomViewVM>> CreateRoomView(RoomViewCreateVM vm)
        => PostGeneric<RoomViewVM, RoomViewCreateVM>("create-room-view", vm);

    // =========================================================================
    // 5. UPDATE (Dùng UpdateVM + ID)
    // =========================================================================
    public Task<ApiResponse<ServiceVM>> UpdateService(int id, ServiceUpdateVM vm)
        => PutGenericWithSlug<ServiceVM, ServiceUpdateVM>(id, vm, isService: true);

    public Task<ApiResponse<PolicyVM>> UpdatePolicy(int id, PolicyUpdateVM vm)
        => PutGeneric<PolicyVM, PolicyUpdateVM>("update-policy", id, vm);

    public Task<ApiResponse<AmenityVM>> UpdateAmenity(int id, AmenityUpdateVM vm)
        => PutGeneric<AmenityVM, AmenityUpdateVM>("update-amenity", id, vm);

    public Task<ApiResponse<RoomQualityVM>> UpdateRoomQuality(int id, RoomQualityUpdateVM vm)
        => PutGeneric<RoomQualityVM, RoomQualityUpdateVM>("update-room-quality", id, vm);

    public Task<ApiResponse<UnitTypeVM>> UpdateUnitType(int id, UnitTypeUpdateVM vm)
        => PutGeneric<UnitTypeVM, UnitTypeUpdateVM>("update-unit-type", id, vm);

    public Task<ApiResponse<BedTypeVM>> UpdateBedType(int id, BedTypeUpdateVM vm)
        => PutGeneric<BedTypeVM, BedTypeUpdateVM>("update-bed-type", id, vm);

    public Task<ApiResponse<RoomViewVM>> UpdateRoomView(int id, RoomViewUpdateVM vm)
        => PutGeneric<RoomViewVM, RoomViewUpdateVM>("update-room-view", id, vm);

    // ==========================================
    // 6. PRIVATE HELPERS
    // ==========================================

    // Helper: Ghép chuỗi lấy danh sách Typed
    private Task<ApiResponse<PagedManageResult<T>>> GetGenericTyped<T>(string endpoint, int? typeId, PagingRequest paging)
    {
        var queryParams = new List<string>();

        if (typeId.HasValue) queryParams.Add($"typeId={typeId}");

        // Paging luôn có giá trị mặc định ở ViewModel, nhưng check cho an toàn
        queryParams.Add($"pageIndex={paging.PageIndex}");
        queryParams.Add($"pageSize={paging.PageSize}");

        var url = $"{endpoint}?{string.Join("&", queryParams)}";
        return _http.GetApiAsync<PagedManageResult<T>>(url);
    }

    // Helper: Ghép chuỗi lấy danh sách Non-Typed (Room Attributes)
    private Task<ApiResponse<PagedManageResult<T>>> GetAttributePaged<T>(RoomAttributeType type, PagingRequest paging, int? typeId = null)
    {
        // Đây là chỗ quan trọng nhất cần sửa để khớp với BE mới
        var url = $"v1/admin/Management/room-attribute/get-paged-data" +
                  $"?type={(int)type}" +
                  $"&pageIndex={paging.PageIndex}" +
                  $"&pageSize={paging.PageSize}";

        if (typeId.HasValue) url += $"&typeId={typeId}";

        return _http.GetApiAsync<PagedManageResult<T>>(url);
    }

    // Helper: Logic Slug đặc biệt cho Service
    private string GetServiceSlug<T>(T vm)
    {
        return vm switch
        {
            // Gom tất cả những gì liên quan đến Standard về 1 slug duy nhất
            ServiceStandardVM _ or
            ServiceStandardCreateVM _ or
            ServiceStandardUpdateVM _ => "standard-service",

            // Gom tất cả những gì liên quan đến Airport về 1 slug duy nhất
            ServiceAirportTransferVM _ or
            ServiceAirportCreateVM _ or
            ServiceAirportUpdateVM _ => "airport-transfer-service",
            _ => throw new NotSupportedException($"Chưa hỗ trợ loại service: {vm?.GetType().Name}")
        };
    }

    // 1. Helper Post/Put đặc biệt cho Service (có xử lý slug)
    private Task<ApiResponse<TResponse>> PostGenericWithSlug<TResponse, TRequest>(TRequest vm, bool isService)
    {
        var slug = isService ? GetServiceSlug(vm) : "";
        var url = string.IsNullOrEmpty(slug) ? $"{BaseUrl}/create" : $"{BaseUrl}/create-{slug}";

        // [TYPE SAFE] Truyền cả TResponse và TRequest vào
        return _http.PostApiAsync<TResponse, TRequest>(url, vm);
    }

    private Task<ApiResponse<TResponse>> PutGenericWithSlug<TResponse, TRequest>(int id, TRequest vm, bool isService)
    {
        var slug = isService ? GetServiceSlug(vm) : "";
        var url = string.IsNullOrEmpty(slug) ? $"{BaseUrl}/update/{id}" : $"{BaseUrl}/update-{slug}/{id}";

        // [TYPE SAFE] Truyền cả TResponse và TRequest vào
        return _http.PutApiAsync<TResponse, TRequest>(url, vm);
    }

    // Helper Generic chuẩn
    private Task<ApiResponse<TResponse>> PostGeneric<TResponse, TRequest>(string endpoint, TRequest vm)
     => _http.PostApiAsync<TResponse, TRequest>($"{BaseUrl}/{endpoint}", vm);

    private Task<ApiResponse<TResponse>> PutGeneric<TResponse, TRequest>(string endpoint, int id, TRequest vm)
     => _http.PutApiAsync<TResponse, TRequest>($"{BaseUrl}/{endpoint}/{id}", vm);

    private Task<ApiResponse<bool>> DeleteGeneric(string endpoint, int id)
        => _http.DeleteApiAsync<bool>($"v1/admin/Management/{endpoint}/{id}");
}