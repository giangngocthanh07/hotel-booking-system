namespace HotelBooking.application.DTOs.Request;

/// <summary>
/// Các trạng thái của yêu cầu nâng cấp Owner
/// </summary>
public static class UpgradeRequestStatusConst
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string None = "None";

    /// <summary>
    /// Kiểm tra xem status có hợp lệ không
    /// </summary>
    public static bool IsValid(string? status)
    {
        return status == Pending || status == Approved || status == Rejected || status == Cancelled || status == None;
    }

    /// <summary>
    /// Lấy danh sách tất cả status hợp lệ
    /// </summary>
    public static List<string> GetAll() => new() { Pending, Approved, Rejected, Cancelled, None };
}
