using System.Net.Http.Headers;
using HotelBooking.webapp.Helpers.Common;
using HotelBooking.webapp.ViewModels.Request;
using HotelBooking.webapp.ViewModels.Request.Base;

namespace HotelBooking.webapp.Services.Interface;

/// <summary>
/// Facade Pattern: Unified Interface for the Request Service.
/// Provides a centralized entry point to manage ALL request types.
/// 
/// Generic Methods enable:
/// - Type Safety: Eliminates the need for dynamic/object casting.
/// - Polymorphism: Same method logic applied across multiple request types.
/// - Scalability: New request types can be added without modifying the interface.
/// 
/// Inherits from ITokenService to support generic AdminPageBase<TService> functionality.
/// </summary>
public interface IRequestService : ITokenService
{
    // ==========================================
    // GENERIC METHODS (Polymorphism)
    // ==========================================

    /// <summary>
    /// Retrieves a paginated list of requests - Generic for all types.
    /// </summary>
    /// <typeparam name="T">Request Type (UpgradeRequestVM, HotelApprovalRequestVM, etc.)</typeparam>
    Task<ApiResponse<PagedResult<T>>> GetRequestsAsync<T>(
        RequestType type,
        int pageIndex = 1,
        int pageSize = 10,
        string? status = null) where T : BaseRequestVM;

    /// <summary>
    /// Retrieves specific request details by ID - Generic.
    /// </summary>
    Task<ApiResponse<T>> GetRequestByIdAsync<T>(RequestType type, int id) where T : BaseRequestVM;

    /// <summary>
    /// Approves a specific request - Generic.
    /// </summary>
    Task<ApiResponse<T>> ApproveRequestAsync<T>(RequestType type, int id) where T : BaseRequestVM;

    /// <summary>
    /// Rejects a specific request - Generic.
    /// </summary>
    Task<ApiResponse<T>> RejectRequestAsync<T>(RequestType type, int id) where T : BaseRequestVM;

    // ==========================================
    // OVERVIEW / STATISTICS (Dashboard)
    // ==========================================

    /// <summary>
    /// Fetches overall statistics across all request types.
    /// </summary>
    Task<ApiResponse<RequestStatsVM>> GetStatsAsync();

    /// <summary>
    /// Fetches statistics for a specific request type.
    /// </summary>
    Task<ApiResponse<RequestTypeStatsVM>> GetStatsByTypeAsync(RequestType type);

    /// <summary>
    /// Retrieves recent requests for dashboard widgets.
    /// </summary>
    Task<ApiResponse<List<RecentRequestVM>>> GetRecentRequestsAsync(int count = 10);

    // ==========================================
    // SHORTCUT METHODS (Convenience)
    // ==========================================

    /// <summary>
    /// [Shortcut] Retrieves paginated Upgrade Owner requests.
    /// </summary>
    Task<ApiResponse<PagedResult<UpgradeRequestVM>>> GetUpgradeRequestsAsync(
        int pageIndex = 1,
        int pageSize = 10,
        string? status = null);

    /// <summary>
    /// [Shortcut] Retrieves specific Upgrade Owner request details.
    /// </summary>
    Task<ApiResponse<UpgradeRequestVM>> GetUpgradeRequestByIdAsync(int id);

    /// <summary>
    /// [Shortcut] Approves an Upgrade Owner request.
    /// </summary>
    Task<ApiResponse<UpgradeRequestVM>> ApproveUpgradeRequestAsync(int id);

    /// <summary>
    /// [Shortcut] Rejects an Upgrade Owner request.
    /// </summary>
    Task<ApiResponse<UpgradeRequestVM>> RejectUpgradeRequestAsync(int id);
}

/// <summary>
/// Facade Implementation: Centralized service to manage ALL request workflows.
/// 
/// Design Principles:
/// - Generic methods for strong-typed polymorphism.
/// - Shortcut methods for developer convenience.
/// - Utilizes RequestType enum mapping to resolve API endpoints.
/// 
/// Extension:
/// - To add a new request type, simply update the RequestType enum.
/// - API paths are resolved automatically via RequestTypeExtensions.GetApiPath().
/// </summary>
public class RequestService : IRequestService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "v1/admin";

    public RequestService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("HotelBookingAPI");
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    // ==========================================
    // GENERIC METHODS (Polymorphism Core)
    // ==========================================

    public Task<ApiResponse<PagedResult<T>>> GetRequestsAsync<T>(
        RequestType type,
        int pageIndex = 1,
        int pageSize = 10,
        string? status = null) where T : BaseRequestVM
    {
        var url = BuildPaginatedUrl(type.GetApiPath(), pageIndex, pageSize, status);
        return _http.GetApiAsync<PagedResult<T>>(url);
    }

    public Task<ApiResponse<T>> GetRequestByIdAsync<T>(RequestType type, int id) where T : BaseRequestVM
    {
        var url = $"{BaseUrl}/{type.GetApiPath()}/{id}";
        return _http.GetApiAsync<T>(url);
    }

    public Task<ApiResponse<T>> ApproveRequestAsync<T>(RequestType type, int id) where T : BaseRequestVM
    {
        var url = $"{BaseUrl}/{type.GetApiPath()}/{id}/approve";
        return _http.PostApiAsync<T>(url);
    }

    public Task<ApiResponse<T>> RejectRequestAsync<T>(RequestType type, int id) where T : BaseRequestVM
    {
        var url = $"{BaseUrl}/{type.GetApiPath()}/{id}/reject";
        return _http.PostApiAsync<T>(url);
    }

    // ==========================================
    // OVERVIEW / STATISTICS
    // ==========================================

    public Task<ApiResponse<RequestStatsVM>> GetStatsAsync()
        => _http.GetApiAsync<RequestStatsVM>($"{BaseUrl}/requests/stats");

    public Task<ApiResponse<RequestTypeStatsVM>> GetStatsByTypeAsync(RequestType type)
        => _http.GetApiAsync<RequestTypeStatsVM>($"{BaseUrl}/{type.GetApiPath()}/stats");

    public Task<ApiResponse<List<RecentRequestVM>>> GetRecentRequestsAsync(int count = 10)
        => _http.GetApiAsync<List<RecentRequestVM>>($"{BaseUrl}/requests/recent?count={count}");

    // ==========================================
    // SHORTCUT METHODS (Convenience)
    // ==========================================

    public Task<ApiResponse<PagedResult<UpgradeRequestVM>>> GetUpgradeRequestsAsync(
        int pageIndex = 1,
        int pageSize = 10,
        string? status = null)
        => GetRequestsAsync<UpgradeRequestVM>(RequestType.UpgradeOwner, pageIndex, pageSize, status);

    public Task<ApiResponse<UpgradeRequestVM>> GetUpgradeRequestByIdAsync(int id)
        => GetRequestByIdAsync<UpgradeRequestVM>(RequestType.UpgradeOwner, id);

    public Task<ApiResponse<UpgradeRequestVM>> ApproveUpgradeRequestAsync(int id)
        => ApproveRequestAsync<UpgradeRequestVM>(RequestType.UpgradeOwner, id);

    public Task<ApiResponse<UpgradeRequestVM>> RejectUpgradeRequestAsync(int id)
        => RejectRequestAsync<UpgradeRequestVM>(RequestType.UpgradeOwner, id);

    // ==========================================
    // PRIVATE HELPERS
    // ==========================================

    private string BuildPaginatedUrl(string path, int pageIndex, int pageSize, string? status)
    {
        var queryParams = new List<string>
        {
            $"pageIndex={pageIndex}",
            $"pageSize={pageSize}"
        };

        if (!string.IsNullOrEmpty(status))
        {
            queryParams.Add($"status={status}");
        }

        return $"{BaseUrl}/{path}?{string.Join("&", queryParams)}";
    }
}