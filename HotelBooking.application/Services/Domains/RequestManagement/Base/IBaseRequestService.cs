using HotelBooking.application.DTOs.Request.Base;
using HotelBooking.application.DTOs.Request.UpgradeRequest;

namespace HotelBooking.application.Services.Domains.RequestManagement.Base;

// ==========================================
// 1. ADMIN BASE INTERFACE
// ==========================================

/// <summary>
/// Generic Base Interface for Admin Request Services.
/// </summary>
/// <typeparam name="T">DTOs that inherit from BaseRequestDTO</typeparam>
public interface IBaseAdminRequestService<T> where T : BaseRequestDTO
{
    /// <summary>
    /// Get paged list of requests (Filter, Sort, Paging)
    /// </summary>
    Task<ApiResponse<PagedResult<T>>> GetPagedRequestsAsync(PagingRequest pagingRequest, string? status = null);

    /// <summary>
    /// Get request details by ID
    /// </summary>
    Task<ApiResponse<T>> GetByRequestIdAsync(int requestId);

    /// <summary>
    /// Approve request
    /// </summary>
    Task<ApiResponse<bool>> ApproveRequestAsync(int requestId, int adminId);

    /// <summary>
    /// Reject request
    /// </summary>
    Task<ApiResponse<bool>> RejectRequestAsync(int requestId, int adminId);

    /// <summary>
    /// Get a list of all available statuses for filtering
    /// </summary>
    Task<ApiResponse<List<string>>> GetAllStatusesAsync();
}

// ==========================================
// 2. BASE INTERFACE
// ==========================================

/// <summary>
/// Generic Base Interface for Request Services.
/// </summary>
/// <typeparam name="T">DTO type</typeparam>
/// <typeparam name="TCreate">DTO type for creating new</typeparam>
public interface IBaseUserRequestService<T, TCreate>
    where T : BaseRequestDTO
    where TCreate : class
{
    /// <summary>
    /// Create new request
    /// </summary>
    Task<ApiResponse<T>> CreateRequestAsync(int userId, TCreate createDto);

    /// <summary>
    /// Cancel request (if pending)
    /// </summary>
    Task<ApiResponse<bool>> CancelRequestAsync(int userId, int requestId);

    /// <summary>
    /// Get all requests of a specific user
    /// </summary>
    Task<ApiResponse<List<T>>> GetMyRequestsAsync(int userId);
}
