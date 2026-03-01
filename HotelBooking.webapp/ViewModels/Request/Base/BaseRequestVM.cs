namespace HotelBooking.webapp.ViewModels.Request.Base;

/// <summary>
/// Interface marker cho tất cả Request ViewModels.
/// Định nghĩa các properties chung mà MỌI loại request đều phải có.
/// Sử dụng Polymorphism: cho phép xử lý chung các loại request khác nhau.
/// </summary>
public interface IRequestVM
{
    /// <summary>
    /// ID của request - Primary Key
    /// </summary>
    int RequestId { get; set; }

    /// <summary>
    /// Loại request (UpgradeOwner, HotelApproval, etc.)
    /// </summary>
    RequestType Type { get; }

    /// <summary>
    /// Tên hiển thị của loại request (tiếng Việt)
    /// </summary>
    string TypeDisplay { get; }

    /// <summary>
    /// Trạng thái hiện tại của request
    /// </summary>
    string Status { get; set; }

    /// <summary>
    /// Thời gian tạo request
    /// </summary>
    DateTime RequestedAt { get; set; }

    /// <summary>
    /// ID người xử lý (Admin)
    /// </summary>
    int? ProcessedBy { get; set; }

    /// <summary>
    /// Thời gian xử lý
    /// </summary>
    DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Tên người yêu cầu (để hiển thị nhanh)
    /// </summary>
    string RequesterName { get; }

    /// <summary>
    /// Kiểm tra có thể Approve không
    /// </summary>
    bool CanApprove => Status == RequestStatusConst.Pending;

    /// <summary>
    /// Kiểm tra có thể Reject không
    /// </summary>
    bool CanReject => Status == RequestStatusConst.Pending;
}

/// <summary>
/// Enum định nghĩa các loại Request trong hệ thống.
/// Dễ dàng mở rộng khi có loại mới.
/// </summary>
public enum RequestType
{
    UpgradeOwner = 1,
    HotelApproval = 2
    // Thêm loại mới ở đây khi cần
}

/// <summary>
/// Các trạng thái chung của Request
/// </summary>
public static class RequestStatusConst
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string None = "None";

    public static bool IsValid(string? status)
        => status == Pending || status == Approved || status == Rejected || status == Cancelled || status == None;

    public static List<string> GetAll() => new() { Pending, Approved, Rejected, Cancelled };

    public static List<string> GetFilterable() => new() { Pending, Approved, Rejected, Cancelled };
}

/// <summary>
/// Extension methods cho RequestType enum
/// </summary>
public static class RequestTypeExtensions
{
    /// <summary>
    /// Lấy tên hiển thị tiếng Việt
    /// </summary>
    public static string GetDisplayName(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "Nâng cấp Owner",
        RequestType.HotelApproval => "Duyệt khách sạn",
        _ => type.ToString()
    };

    /// <summary>
    /// Lấy API endpoint base path
    /// </summary>
    public static string GetApiPath(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "upgrade-requests",
        RequestType.HotelApproval => "hotel-approvals",
        _ => throw new NotSupportedException($"RequestType {type} is not supported")
    };

    /// <summary>
    /// Lấy icon CSS class
    /// </summary>
    public static string GetIcon(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "fas fa-user-shield",
        RequestType.HotelApproval => "fas fa-hotel",
        _ => "fas fa-file-alt"
    };

    /// <summary>
    /// Lấy màu badge
    /// </summary>
    public static string GetBadgeColor(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "primary",
        RequestType.HotelApproval => "info",
        _ => "secondary"
    };
}


/// <summary>
/// Abstract base class cho tất cả Request ViewModels.
/// Implement các properties chung, để các concrete class chỉ cần định nghĩa properties riêng.
/// 
/// Design Pattern: Template Method + Polymorphism
/// - Common properties được implement ở đây
/// - Abstract properties bắt buộc concrete class phải override
/// - Virtual properties cho phép customize nếu cần
/// </summary>
public abstract class BaseRequestVM : IRequestVM
{
    // ==========================================
    // ABSTRACT PROPERTIES (Mỗi loại request khác nhau)
    // ==========================================

    /// <summary>
    /// Loại request - MỖI concrete class PHẢI định nghĩa
    /// </summary>
    public abstract RequestType Type { get; }

    /// <summary>
    /// Tên người yêu cầu - MỖI concrete class PHẢI định nghĩa
    /// (vì field name có thể khác: UserName, OwnerName, HotelName...)
    /// </summary>
    public abstract string RequesterName { get; }

    // ==========================================
    // COMMON PROPERTIES (Dùng chung cho mọi loại)
    // ==========================================

    public int RequestId { get; set; }
    public string Status { get; set; } = RequestStatusConst.Pending;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }

    // ==========================================
    // COMPUTED PROPERTIES (Tự động tính toán)
    // ==========================================

    /// <summary>
    /// Tên hiển thị của loại request (tiếng Việt)
    /// </summary>
    public string TypeDisplay => Type.GetDisplayName();

    /// <summary>
    /// Kiểm tra có thể Approve không
    /// </summary>
    public bool CanApprove => Status == RequestStatusConst.Pending;

    /// <summary>
    /// Kiểm tra có thể Reject không
    /// </summary>
    public bool CanReject => Status == RequestStatusConst.Pending;

    /// <summary>
    /// CSS class cho badge status
    /// </summary>
    public string StatusBadgeClass => Status switch
    {
        RequestStatusConst.Pending => "bg-warning text-dark",
        RequestStatusConst.Approved => "bg-success",
        RequestStatusConst.Rejected => "bg-danger",
        RequestStatusConst.Cancelled => "bg-secondary",
        _ => "bg-light text-dark"
    };

    /// <summary>
    /// Icon cho status
    /// </summary>
    public string StatusIcon => Status switch
    {
        RequestStatusConst.Pending => "fas fa-clock",
        RequestStatusConst.Approved => "fas fa-check-circle",
        RequestStatusConst.Rejected => "fas fa-times-circle",
        RequestStatusConst.Cancelled => "fas fa-ban",
        _ => "fas fa-question-circle"
    };

    /// <summary>
    /// Thời gian từ khi tạo request (human-readable)
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var span = DateTime.Now - RequestedAt;
            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} ngày trước";
            return RequestedAt.ToString("dd/MM/yyyy");
        }
    }
}
