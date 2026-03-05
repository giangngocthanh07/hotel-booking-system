using HotelBooking.application.DTOs.Request.Base;

namespace HotelBooking.application.DTOs.Request.UpgradeRequest;

/// <summary>
/// DTO for Upgrade Owner Request.
/// Inherits BaseRequestDTO to reuse common properties.
/// Override abstract properties to define specifics of UpgradeRequest.
/// </summary>
public class UpgradeRequestDTO : BaseRequestDTO
{
    // ==========================================
    // OVERRIDE ABSTRACT (Mandatory to define)
    // ==========================================

    /// <summary>
    /// Type is UpgradeOwner
    /// </summary>
    public override RequestType Type => RequestType.UpgradeOwner;

    /// <summary>
    /// Requester name = FullName of User
    /// </summary>
    public override string RequesterName => FullName;

    // ==========================================
    // SPECIFIC PROPERTIES (Specific for Upgrade)
    // ==========================================

    /// <summary>
    /// User ID of the requester
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Login username
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Business address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Tax code
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;
}