using HotelBooking.application.DTOs.Request.Base;

namespace HotelBooking.application.Services.Domains.RequestManagement.Base;

/// <summary>
/// Generic Base Interface cho tất cả Request Services.
/// Định nghĩa các operations chung mà mọi loại request đều có.
/// 
/// Design Pattern: Template Method via Interface
/// - Generic constraint đảm bảo type safety
/// - Mỗi concrete service chỉ cần implement interface này
/// - Dễ dàng mở rộng cho loại request mới
/// </summary>
/// <typeparam name="T">Loại DTO kế thừa từ BaseRequestDTO</typeparam>
public interface IBaseRequestService<T> where T : BaseRequestDTO
{
    // ==========================================
    // ADMIN OPERATIONS (Common cho tất cả)
    // ==========================================

    /// <summary>
    /// Lấy danh sách requests có phân trang
    /// </summary>
    Task<ApiResponse<PagedResult<T>>> GetPagedRequestsAsync(PagingRequest pagingRequest, string? status = null);

    /// <summary>
    /// Lấy chi tiết request theo ID
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
    /// Lấy danh sách các status
    /// </summary>
    Task<ApiResponse<List<string>>> GetAllStatusesAsync();
}

/// <summary>
/// Interface mở rộng cho các service có thêm operations riêng.
/// Dùng khi cần thêm methods đặc thù cho từng loại request.
/// </summary>
/// <typeparam name="T">Loại DTO</typeparam>
/// <typeparam name="TCreate">Loại DTO để tạo mới</typeparam>
public interface IRequestServiceWithCreate<T, TCreate> : IBaseRequestService<T>
    where T : BaseRequestDTO
{
    /// <summary>
    /// Tạo request mới
    /// </summary>
    Task<ApiResponse<bool>> CreateRequestAsync(int userId, TCreate createDto);

    /// <summary>
    /// Hủy request (nếu đang pending)
    /// </summary>
    Task<ApiResponse<bool>> CancelRequestAsync(int requestId);
}
