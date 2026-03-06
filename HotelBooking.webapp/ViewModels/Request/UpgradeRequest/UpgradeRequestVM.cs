using HotelBooking.webapp.ViewModels.Request.Base;

namespace HotelBooking.webapp.ViewModels.Request;

/// <summary>
/// ViewModel specifically for Owner Upgrade requests.
/// Inherits from BaseRequestVM to reuse common properties and logic.
/// Overrides abstract properties to define UpgradeRequest specific behavior.
/// </summary>
public class UpgradeRequestVM : BaseRequestVM
{
    // ==========================================
    // OVERRIDE ABSTRACT PROPERTIES
    // ==========================================

    /// <summary>
    /// Explicitly sets the request type to UpgradeOwner.
    /// </summary>
    public override RequestType Type => RequestType.UpgradeOwner;

    /// <summary>
    /// Maps the requester's name to the user's FullName.
    /// </summary>
    public override string RequesterName => FullName;

    // ==========================================
    // SPECIFIC UPGRADE PROPERTIES
    // ==========================================

    /// <summary>
    /// Unique identifier of the requesting user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// User's login username.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// User's complete legal name.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User's contact email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's contact phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// The registered business address provided for the upgrade.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Business Tax Identification Number (TIN).
    /// </summary>
    public string TaxCode { get; set; } = string.Empty;
}