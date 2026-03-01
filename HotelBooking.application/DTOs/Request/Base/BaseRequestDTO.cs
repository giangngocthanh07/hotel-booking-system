namespace HotelBooking.application.DTOs.Request.Base;
/// <summary>
/// Interface marker cho tất cả Request DTOs.
/// Định nghĩa các properties chung mà MỌI loại request đều phải có.
/// Sử dụng Polymorphism: cho phép xử lý chung các loại request khác nhau.
/// </summary>
public interface IRequestDTO
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
    /// Tên hiển thị của loại request
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
    bool CanApprove { get; }

    /// <summary>
    /// Kiểm tra có thể Reject không
    /// </summary>
    bool CanReject { get; }
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
/// Các trạng thái chung của Request - dùng cho tất cả loại
/// </summary>
public static class RequestStatusConst
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string None = "None";

    /// <summary>
    /// Kiểm tra status có hợp lệ không
    /// </summary>
    public static bool IsValid(string? status)
        => status == Pending || status == Approved || status == Rejected || status == Cancelled || status == None;

    /// <summary>
    /// Lấy tất cả status
    /// </summary>
    public static List<string> GetAll() => new() { Pending, Approved, Rejected, Cancelled, None };

    /// <summary>
    /// Lấy status có thể filter (không bao gồm None)
    /// </summary>
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
    /// Lấy tên hiển thị tiếng Anh
    /// </summary>
    public static string GetDisplayNameEn(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "Upgrade Owner",
        RequestType.HotelApproval => "Hotel Approval",
        _ => type.ToString()
    };

    /// <summary>
    /// Parse từ string sang RequestType
    /// </summary>
    public static RequestType? FromString(string? typeString) => typeString?.ToLower() switch
    {
        "upgradeowner" => RequestType.UpgradeOwner,
        "upgrade_owner" => RequestType.UpgradeOwner,
        "hotelapproval" => RequestType.HotelApproval,
        "hotel_approval" => RequestType.HotelApproval,
        _ => null
    };
}

/// <summary>
/// Abstract base class cho tất cả Request DTOs.
/// Implement các properties chung, để các concrete class chỉ cần định nghĩa properties riêng.
/// 
/// Design Pattern: Template Method + Polymorphism
/// - Common properties được implement ở đây
/// - Abstract properties bắt buộc concrete class phải override
/// </summary>
public abstract class BaseRequestDTO : IRequestDTO
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
    /// Tên hiển thị của loại request
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
    /// Kiểm tra request đã được xử lý chưa
    /// </summary>
    public bool IsProcessed => Status != RequestStatusConst.Pending;

    /// <summary>
    /// Kiểm tra request thành công
    /// </summary>
    public bool IsSuccessful => Status == RequestStatusConst.Approved;
}
