using System.Net.Http.Headers;
using HotelBooking.webapp.ViewModels.Request;
using HotelBooking.webapp.ViewModels.Request.Base;

namespace HotelBooking.webapp.Services.Interface;

/// <summary>
/// Facade Pattern: Interface cho Request Service.
/// Cung cấp một interface thống nhất để quản lý TẤT CẢ loại requests.
/// 
/// Generic Methods cho phép:
/// - Type Safety (không dùng dynamic/object)
/// - Polymorphism (cùng method, nhiều loại request)
/// - Dễ mở rộng (thêm loại request mới không cần sửa interface)
/// </summary>
public interface IRequestService
{
    /// <summary>
    /// Set Authorization token cho HTTP requests
    /// </summary>
    void SetToken(string token);

    // ==========================================
    // GENERIC METHODS (Polymorphism)
    // ==========================================

    /// <summary>
    /// Lấy danh sách requests có phân trang - Generic cho mọi loại.
    /// </summary>
    /// <typeparam name="T">Loại Request (UpgradeRequestVM, HotelApprovalRequestVM...)</typeparam>
    Task<ApiResponse<PagedResult<T>>> GetRequestsAsync<T>(
        RequestType type,
        int pageIndex = 1,
        int pageSize = 10,
        string? status = null) where T : BaseRequestVM;

    /// <summary>
    /// Lấy chi tiết request theo ID - Generic.
    /// </summary>
    Task<ApiResponse<T>> GetRequestByIdAsync<T>(RequestType type, int id) where T : BaseRequestVM;

    /// <summary>
    /// Approve request - Generic.
    /// </summary>
    Task<ApiResponse<T>> ApproveRequestAsync<T>(RequestType type, int id) where T : BaseRequestVM;

    /// <summary>
    /// Reject request - Generic.
    /// </summary>
    Task<ApiResponse<T>> RejectRequestAsync<T>(RequestType type, int id) where T : BaseRequestVM;

    // ==========================================
    // OVERVIEW / STATS (Dashboard)
    // ==========================================

    /// <summary>
    /// Lấy thống kê tổng quan tất cả loại requests
    /// </summary>
    Task<ApiResponse<RequestStatsVM>> GetStatsAsync();

    /// <summary>
    /// Lấy thống kê theo loại request cụ thể
    /// </summary>
    Task<ApiResponse<RequestTypeStatsVM>> GetStatsByTypeAsync(RequestType type);

    /// <summary>
    /// Lấy requests gần đây (cho dashboard widget)
    /// </summary>
    Task<ApiResponse<List<RecentRequestVM>>> GetRecentRequestsAsync(int count = 10);

    // ==========================================
    // SHORTCUT METHODS (Tiện lợi, không bắt buộc)
    // ==========================================

    /// <summary>
    /// [Shortcut] Lấy danh sách Upgrade Requests
    /// </summary>
    Task<ApiResponse<PagedResult<UpgradeRequestVM>>> GetUpgradeRequestsAsync(
        int pageIndex = 1,
        int pageSize = 10,
        string? status = null);

    /// <summary>
    /// [Shortcut] Lấy chi tiết Upgrade Request
    /// </summary>
    Task<ApiResponse<UpgradeRequestVM>> GetUpgradeRequestByIdAsync(int id);

    /// <summary>
    /// [Shortcut] Approve Upgrade Request
    /// </summary>
    Task<ApiResponse<UpgradeRequestVM>> ApproveUpgradeRequestAsync(int id);

    /// <summary>
    /// [Shortcut] Reject Upgrade Request
    /// </summary>
    Task<ApiResponse<UpgradeRequestVM>> RejectUpgradeRequestAsync(int id);
}

/// <summary>
/// Facade Pattern: Service thống nhất quản lý TẤT CẢ loại requests.
/// 
/// Design:
/// - Generic methods cho polymorphism (strong typed)
/// - Shortcut methods cho convenience
/// - Sử dụng RequestType enum để map đến API endpoints
/// 
/// Mở rộng:
/// - Thêm loại request mới: chỉ cần thêm vào RequestType enum
/// - API path tự động lấy từ RequestTypeExtensions.GetApiPath()
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
    // OVERVIEW / STATS
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