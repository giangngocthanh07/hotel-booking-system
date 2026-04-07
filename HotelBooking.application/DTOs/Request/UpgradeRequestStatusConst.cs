using HotelBooking.application.DTOs.Request.Base;

namespace HotelBooking.application.DTOs.Request;

/// <summary>
/// Status constants for Upgrade Request.
/// [OBSOLETE] Use HotelBooking.application.DTOs.Request.Base.RequestStatusConst instead.
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
    /// Check if status is valid
    /// </summary>
    [Obsolete("Use RequestStatusConst.IsValid instead")]
    public static bool IsValid(string? status) => RequestStatusConst.IsValid(status);

    /// <summary>
    /// Get all valid statuses
    /// </summary>
    [Obsolete("Use RequestStatusConst.GetAll instead")]
    public static List<string> GetAll() => RequestStatusConst.GetAll();
}
