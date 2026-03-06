namespace HotelBooking.webapp.ViewModels.Request.Base;

/// <summary>
/// Marker interface for all Request ViewModels.
/// Defines shared properties that EVERY request type must implement.
/// Enables Polymorphism for unified handling of different request types.
/// </summary>
public interface IRequestVM
{
    /// <summary>
    /// Unique identifier for the request - Primary Key.
    /// </summary>
    int RequestId { get; set; }

    /// <summary>
    /// The type of request (UpgradeOwner, HotelApproval, etc.).
    /// </summary>
    RequestType Type { get; }

    /// <summary>
    /// Friendly display name for the request type.
    /// </summary>
    string TypeDisplay { get; }

    /// <summary>
    /// Current processing status of the request.
    /// </summary>
    string Status { get; set; }

    /// <summary>
    /// Timestamp when the request was created.
    /// </summary>
    DateTime RequestedAt { get; set; }

    /// <summary>
    /// ID of the administrator who processed the request.
    /// </summary>
    int? ProcessedBy { get; set; }

    /// <summary>
    /// Timestamp when the request was processed.
    /// </summary>
    DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Name of the user making the request.
    /// </summary>
    string RequesterName { get; }

    /// <summary>
    /// Logic to determine if the request can be approved.
    /// </summary>
    bool CanApprove => Status == RequestStatusConst.Pending;

    /// <summary>
    /// Logic to determine if the request can be rejected.
    /// </summary>
    bool CanReject => Status == RequestStatusConst.Pending;
}

/// <summary>
/// Defines the available types of Requests in the system.
/// </summary>
public enum RequestType
{
    UpgradeOwner = 1,
    HotelApproval = 2
    // Add new types here as the system scales
}

/// <summary>
/// Global constants for Request statuses.
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
/// Extension methods for the RequestType enum to resolve display and API metadata.
/// </summary>
public static class RequestTypeExtensions
{
    /// <summary>
    /// Resolves the user-friendly display name.
    /// </summary>
    public static string GetDisplayName(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "Owner Upgrade",
        RequestType.HotelApproval => "Hotel Approval",
        _ => type.ToString()
    };

    /// <summary>
    /// Resolves the base path for API endpoints.
    /// </summary>
    public static string GetApiPath(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "upgrade-requests",
        RequestType.HotelApproval => "hotel-approvals",
        _ => throw new NotSupportedException($"RequestType {type} is not supported")
    };

    /// <summary>
    /// Resolves the CSS icon class for UI components.
    /// </summary>
    public static string GetIcon(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "fas fa-user-shield",
        RequestType.HotelApproval => "fas fa-hotel",
        _ => "fas fa-file-alt"
    };

    /// <summary>
    /// Resolves the Bootstrap badge color class.
    /// </summary>
    public static string GetBadgeColor(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "primary",
        RequestType.HotelApproval => "info",
        _ => "secondary"
    };
}

/// <summary>
/// Abstract base class for all Request ViewModels.
/// Implements common properties using the Template Method pattern.
/// </summary>
public abstract class BaseRequestVM : IRequestVM
{
    // ==========================================
    // ABSTRACT PROPERTIES (Type-Specific)
    // ==========================================

    public abstract RequestType Type { get; }
    public abstract string RequesterName { get; }

    // ==========================================
    // COMMON PROPERTIES
    // ==========================================

    public int RequestId { get; set; }
    public string Status { get; set; } = RequestStatusConst.Pending;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }

    // ==========================================
    // COMPUTED PROPERTIES (UI Logic)
    // ==========================================

    public string TypeDisplay => Type.GetDisplayName();
    public bool CanApprove => Status == RequestStatusConst.Pending;
    public bool CanReject => Status == RequestStatusConst.Pending;

    public string StatusBadgeClass => Status switch
    {
        RequestStatusConst.Pending => "bg-warning text-dark",
        RequestStatusConst.Approved => "bg-success",
        RequestStatusConst.Rejected => "bg-danger",
        RequestStatusConst.Cancelled => "bg-secondary",
        _ => "bg-light text-dark"
    };

    public string StatusIcon => Status switch
    {
        RequestStatusConst.Pending => "fas fa-clock",
        RequestStatusConst.Approved => "fas fa-check-circle",
        RequestStatusConst.Rejected => "fas fa-times-circle",
        RequestStatusConst.Cancelled => "fas fa-ban",
        _ => "fas fa-question-circle"
    };

    /// <summary>
    /// Returns a human-readable relative time string.
    /// </summary>
    public string TimeAgo
    {
        get
        {
            var span = DateTime.Now - RequestedAt;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
            return RequestedAt.ToString("MMM dd, yyyy");
        }
    }
}