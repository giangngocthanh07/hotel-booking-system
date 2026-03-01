using HotelBooking.application.DTOs.Request.Base;

namespace HotelBooking.application.DTOs.Request;

/// <summary>
/// Các trạng thái của yêu cầu nâng cấp Owner.
/// [OBSOLETE] Sử dụng RequestStatusConst từ Base namespace thay thế.
/// Class này giữ lại để backward compatibility.
/// </summary>
[Obsolete("Use HotelBooking.application.DTOs.Request.Base.RequestStatusConst instead")]
public static class UpgradeRequestStatusConst
{
    public const string Pending = RequestStatusConst.Pending;
    public const string Approved = RequestStatusConst.Approved;
    public const string Rejected = RequestStatusConst.Rejected;
    public const string Cancelled = RequestStatusConst.Cancelled;
    public const string None = RequestStatusConst.None;

    /// <summary>
    /// Kiểm tra xem status có hợp lệ không
    /// </summary>
    [Obsolete("Use RequestStatusConst.IsValid instead")]
    public static bool IsValid(string? status) => RequestStatusConst.IsValid(status);

    /// <summary>
    /// Lấy danh sách tất cả status hợp lệ
    /// </summary>
    [Obsolete("Use RequestStatusConst.GetAll instead")]
    public static List<string> GetAll() => RequestStatusConst.GetAll();
}
