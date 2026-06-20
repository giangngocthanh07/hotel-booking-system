using HotelBooking.webapp.Helpers.Common;
using HotelBooking.webapp.Services.Interface;
using HotelBooking.webapp.ViewModels.Admin;

namespace HotelBooking.webapp.Services;

/// <summary>
/// Interface for the Management Service - responsible for entity lifecycle management (Amenities, BedTypes, etc.)
/// Inherits from ITokenService to support the generic AdminPageBase<TService> functionality.
/// </summary>
public interface IManagementService : ITokenService
{
    // 1. NAVIGATION & CONFIGURATION
    Task<ApiResponse<ManageMenuResultVM>> GetManageModuleTypesOnly(ManageModuleEnum module);

    // 2. DATA RETRIEVAL (PAGINATION)
    // --- Typed Group (Entity with TypeId) ---
    Task<ApiResponse<PagedManageResult<ServiceVM>>> GetServicesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<PolicyVM>>> GetPoliciesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<AmenityVM>>> GetAmenitiesByType(int? typeId, PagingRequest paging);
    Task<ApiResponse<PagedManageResult<RoomQualityVM>>> GetRoomQualitiesByType(int? typeId, PagingRequest paging);

    // --- Attributes Group (Direct Entities - Room Attributes) ---
    Task<ApiResponse<PagedManageResult<UnitTypeVM>>> GetUnitTypes(PagingRequest paging);
    Task<ApiResponse<PagedManageResult<BedTypeVM>>> GetBedTypes(PagingRequest paging);
    Task<ApiResponse<PagedManageResult<RoomViewVM>>> GetRoomViews(PagingRequest paging);

    // 3. DELETE OPERATIONS
    Task<ApiResponse<bool>> DeleteService(int id);
    Task<ApiResponse<bool>> DeletePolicy(int id);
    Task<ApiResponse<bool>> DeleteAmenity(int id);
    Task<ApiResponse<bool>> DeleteRoomQuality(int id);
    Task<ApiResponse<bool>> DeleteUnitType(int id);
    Task<ApiResponse<bool>> DeleteBedType(int id);
    Task<ApiResponse<bool>> DeleteRoomView(int id);

    // 4. CREATE OPERATIONS (Accepts CreateVM, Returns OutputVM)
    Task<ApiResponse<ServiceVM>> CreateService(ServiceCreateVM vm);
    Task<ApiResponse<PolicyVM>> CreatePolicy(PolicyCreateVM vm);
    Task<ApiResponse<AmenityVM>> CreateAmenity(AmenityCreateVM vm);
    Task<ApiResponse<RoomQualityVM>> CreateRoomQuality(RoomQualityCreateVM vm);
    Task<ApiResponse<UnitTypeVM>> CreateUnitType(UnitTypeCreateVM vm);
    Task<ApiResponse<BedTypeVM>> CreateBedType(BedTypeCreateVM vm);
    Task<ApiResponse<RoomViewVM>> CreateRoomView(RoomViewCreateVM vm);

    // 5. UPDATE OPERATIONS (Accepts ID and UpdateVM, Returns OutputVM)
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
    // 1. MENU & TYPED DATA RETRIEVAL
    // ==========================================
    public async Task<ApiResponse<ManageMenuResultVM>> GetManageModuleTypesOnly(ManageModuleEnum module)
    {
        // Fetches module types for navigation menus. 
        // Route assumed: get-manage-menu/{module}
        return await _http.GetApiAsync<ManageMenuResultVM>($"{BaseUrl}/get-manage-menu/{module}");
    }

    public async Task<ApiResponse<PagedManageResult<ServiceVM>>> GetServicesByType(int? typeId, PagingRequest paging)
        => await GetGenericTyped<ServiceVM>($"{BaseUrl}/get-service-data", typeId, paging);

    public async Task<ApiResponse<PagedManageResult<PolicyVM>>> GetPoliciesByType(int? typeId, PagingRequest paging)
        => await GetGenericTyped<PolicyVM>($"{BaseUrl}/get-policy-data", typeId, paging);

    public async Task<ApiResponse<PagedManageResult<AmenityVM>>> GetAmenitiesByType(int? typeId, PagingRequest paging)
        => await GetGenericTyped<AmenityVM>($"{BaseUrl}/get-amenity-data", typeId, paging);

    public async Task<ApiResponse<PagedManageResult<RoomQualityVM>>> GetRoomQualitiesByType(int? typeId, PagingRequest paging)
        => await GetGenericTyped<RoomQualityVM>($"{BaseUrl}/get-room-quality-data", typeId, paging);

    // ==========================================
    // 2. ROOM ATTRIBUTES (NON-TYPED DATA)
    // ==========================================
    public async Task<ApiResponse<PagedManageResult<UnitTypeVM>>> GetUnitTypes(PagingRequest paging)
        => await GetAttributePaged<UnitTypeVM>(RoomAttributeType.UnitType, paging);

    public async Task<ApiResponse<PagedManageResult<BedTypeVM>>> GetBedTypes(PagingRequest paging)
        => await GetAttributePaged<BedTypeVM>(RoomAttributeType.BedType, paging);

    public async Task<ApiResponse<PagedManageResult<RoomViewVM>>> GetRoomViews(PagingRequest paging)
        => await GetAttributePaged<RoomViewVM>(RoomAttributeType.RoomView, paging);

    // ==========================================
    // 3. DELETE OPERATIONS
    // ==========================================
    public async Task<ApiResponse<bool>> DeleteService(int id) => await DeleteGeneric("delete-service", id);
    public async Task<ApiResponse<bool>> DeletePolicy(int id) => await DeleteGeneric("delete-policy", id);
    public async Task<ApiResponse<bool>> DeleteAmenity(int id) => await DeleteGeneric("delete-amenity", id);
    public async Task<ApiResponse<bool>> DeleteRoomQuality(int id) => await DeleteGeneric("delete-room-quality", id);
    public async Task<ApiResponse<bool>> DeleteRoomView(int id) => await DeleteGeneric("delete-room-view", id);
    public async Task<ApiResponse<bool>> DeleteBedType(int id) => await DeleteGeneric("delete-bed-type", id);
    public async Task<ApiResponse<bool>> DeleteUnitType(int id) => await DeleteGeneric("delete-unit-type", id);

    // ==========================================
    // 4. CREATE OPERATIONS
    // ==========================================
    public async Task<ApiResponse<ServiceVM>> CreateService(ServiceCreateVM vm)
        => await PostGenericWithSlug<ServiceVM, ServiceCreateVM>(vm, isService: true);

    public async Task<ApiResponse<PolicyVM>> CreatePolicy(PolicyCreateVM vm)
        => await PostGenericWithSlug<PolicyVM, PolicyCreateVM>(vm, isService: false, isPolicy: true);

    public async Task<ApiResponse<AmenityVM>> CreateAmenity(AmenityCreateVM vm)
        => await PostGeneric<AmenityVM, AmenityCreateVM>("create-amenity", vm);

    public async Task<ApiResponse<RoomQualityVM>> CreateRoomQuality(RoomQualityCreateVM vm)
        => await PostGeneric<RoomQualityVM, RoomQualityCreateVM>("create-room-quality", vm);

    public async Task<ApiResponse<UnitTypeVM>> CreateUnitType(UnitTypeCreateVM vm)
        => await PostGeneric<UnitTypeVM, UnitTypeCreateVM>("create-unit-type", vm);

    public async Task<ApiResponse<BedTypeVM>> CreateBedType(BedTypeCreateVM vm)
        => await PostGeneric<BedTypeVM, BedTypeCreateVM>("create-bed-type", vm);

    public async Task<ApiResponse<RoomViewVM>> CreateRoomView(RoomViewCreateVM vm)
        => await PostGeneric<RoomViewVM, RoomViewCreateVM>("create-room-view", vm);

    // ==========================================
    // 5. UPDATE OPERATIONS
    // ==========================================
    public async Task<ApiResponse<ServiceVM>> UpdateService(int id, ServiceUpdateVM vm)
        => await PutGenericWithSlug<ServiceVM, ServiceUpdateVM>(id, vm, isService: true);

    public async Task<ApiResponse<PolicyVM>> UpdatePolicy(int id, PolicyUpdateVM vm)
        => await PutGenericWithSlug<PolicyVM, PolicyUpdateVM>(id, vm, isService: false, isPolicy: true);

    public async Task<ApiResponse<AmenityVM>> UpdateAmenity(int id, AmenityUpdateVM vm)
        => await PutGeneric<AmenityVM, AmenityUpdateVM>("update-amenity", id, vm);

    public async Task<ApiResponse<RoomQualityVM>> UpdateRoomQuality(int id, RoomQualityUpdateVM vm)
        => await PutGeneric<RoomQualityVM, RoomQualityUpdateVM>("update-room-quality", id, vm);

    public async Task<ApiResponse<UnitTypeVM>> UpdateUnitType(int id, UnitTypeUpdateVM vm)
        => await PutGeneric<UnitTypeVM, UnitTypeUpdateVM>("update-unit-type", id, vm);

    public async Task<ApiResponse<BedTypeVM>> UpdateBedType(int id, BedTypeUpdateVM vm)
        => await PutGeneric<BedTypeVM, BedTypeUpdateVM>("update-bed-type", id, vm);

    public async Task<ApiResponse<RoomViewVM>> UpdateRoomView(int id, RoomViewUpdateVM vm)
        => await PutGeneric<RoomViewVM, RoomViewUpdateVM>("update-room-view", id, vm);

    // ==========================================
    // 6. PRIVATE GENERIC HELPERS
    // ==========================================

    // Helper: Construct URL for Typed data retrieval
    private async Task<ApiResponse<PagedManageResult<T>>> GetGenericTyped<T>(string endpoint, int? typeId, PagingRequest paging)
    {
        var queryParams = new List<string>();
        if (typeId.HasValue) queryParams.Add($"typeId={typeId}");
        queryParams.Add($"pageIndex={paging.PageIndex}");
        queryParams.Add($"pageSize={paging.PageSize}");

        var url = $"{endpoint}?{string.Join("&", queryParams)}";
        return await _http.GetApiAsync<PagedManageResult<T>>(url);
    }

    // Helper: Construct URL for Room Attributes (Unified endpoint)
    private async Task<ApiResponse<PagedManageResult<T>>> GetAttributePaged<T>(RoomAttributeType type, PagingRequest paging, int? typeId = null)
    {
        var url = $"{BaseUrl}/room-attribute/get-paged-data" +
                  $"?type={(int)type}" +
                  $"&pageIndex={paging.PageIndex}" +
                  $"&pageSize={paging.PageSize}";

        if (typeId.HasValue) url += $"&typeId={typeId}";

        return await _http.GetApiAsync<PagedManageResult<T>>(url);
    }

    // Helper: Dynamic Slug Resolution for polymorphic Services
    private string GetServiceSlug<T>(T vm)
    {
        return vm switch
        {
            ServiceStandardVM _ or ServiceStandardCreateVM _ or ServiceStandardUpdateVM _
                => "standard-service",
            ServiceAirportTransferVM _ or ServiceAirportCreateVM _ or ServiceAirportUpdateVM _
                => "airport-transfer-service",
            _ => throw new NotSupportedException($"Service type not supported: {vm?.GetType().Name}")
        };
    }

    private string GetPolicySlug<T>(T vm)
    {
        return vm switch
        {
            CheckInOutPolicyCreateVM _ or CheckInOutPolicyUpdateVM _ => "check-in-out-policy",
            CancellationPolicyCreateVM _ or CancellationPolicyUpdateVM _ => "cancellation-policy",
            ChildrenPolicyCreateVM _ or ChildrenPolicyUpdateVM _ => "children-policy",
            PetPolicyCreateVM _ or PetPolicyUpdateVM _ => "pet-policy",
            _ => throw new NotSupportedException($"Policy type not supported: {vm?.GetType().Name}")
        };
    }

    // Helper: Polymorphic POST with slug support
    private async Task<ApiResponse<TResponse>> PostGenericWithSlug<TResponse, TRequest>(TRequest vm, bool isService = false, bool isPolicy = false)
    {
        var slug = isService ? GetServiceSlug(vm) : isPolicy ? GetPolicySlug(vm) : "";
        var url = string.IsNullOrEmpty(slug) ? $"{BaseUrl}/create" : $"{BaseUrl}/create-{slug}";
        return await _http.PostApiAsync<TResponse, TRequest>(url, vm);
    }

    // Helper: Polymorphic PUT with slug support
    private async Task<ApiResponse<TResponse>> PutGenericWithSlug<TResponse, TRequest>(int id, TRequest vm, bool isService = false, bool isPolicy = false)
    {
        var slug = isService ? GetServiceSlug(vm) : isPolicy ? GetPolicySlug(vm) : "";
        var url = string.IsNullOrEmpty(slug) ? $"{BaseUrl}/update/{id}" : $"{BaseUrl}/update-{slug}/{id}";
        return await _http.PutApiAsync<TResponse, TRequest>(url, vm);
    }

    private async Task<ApiResponse<TResponse>> PostGeneric<TResponse, TRequest>(string endpoint, TRequest vm)
     => await _http.PostApiAsync<TResponse, TRequest>($"{BaseUrl}/{endpoint}", vm);

    private async Task<ApiResponse<TResponse>> PutGeneric<TResponse, TRequest>(string endpoint, int id, TRequest vm)
     => await _http.PutApiAsync<TResponse, TRequest>($"{BaseUrl}/{endpoint}/{id}", vm);

    private async Task<ApiResponse<bool>> DeleteGeneric(string endpoint, int id)
        => await _http.DeleteApiAsync<bool>($"{BaseUrl}/{endpoint}/{id}");
}