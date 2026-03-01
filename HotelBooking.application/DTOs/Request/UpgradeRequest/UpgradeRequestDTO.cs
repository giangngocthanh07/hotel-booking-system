using HotelBooking.application.DTOs.Request.Base;

namespace HotelBooking.application.DTOs.Request.UpgradeRequest;

/// <summary>
/// DTO cho Upgrade Owner Request.
/// Kế thừa BaseRequestDTO để tái sử dụng common properties.
/// Override abstract properties để định nghĩa đặc thù của UpgradeRequest.
/// </summary>
public class UpgradeRequestDTO : BaseRequestDTO
{
    // ==========================================
    // OVERRIDE ABSTRACT (Bắt buộc define)
    // ==========================================

    /// <summary>
    /// Loại là UpgradeOwner
    /// </summary>
    public override RequestType Type => RequestType.UpgradeOwner;

    /// <summary>
    /// Tên người yêu cầu = FullName của User
    /// </summary>
    public override string RequesterName => FullName;

    // ==========================================
    // SPECIFIC PROPERTIES (Riêng cho Upgrade)
    // ==========================================

    /// <summary>
    /// User ID của người yêu cầu
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Username đăng nhập
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Họ tên đầy đủ
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Số điện thoại
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Địa chỉ kinh doanh
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Mã số thuế
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;
}