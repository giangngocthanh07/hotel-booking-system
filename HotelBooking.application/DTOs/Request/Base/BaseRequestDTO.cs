namespace HotelBooking.application.DTOs.Request.Base;
/// <summary>
/// Interface marker for all Request DTOs.
/// Defines common properties that ALL types of requests must have.
/// Uses Polymorphism: allows common handling of different types of requests.
/// </summary>
public interface IRequestDTO
{
    /// <summary>
    /// Request ID - Primary Key
    /// </summary>
    int RequestId { get; set; }

    /// <summary>
    /// Request type (UpgradeOwner, HotelApproval, etc.)
    /// </summary>
    RequestType Type { get; }

    /// <summary>
    /// Display name of the request type
    /// </summary>
    string TypeDisplay { get; }

    /// <summary>
    /// Current status of the request
    /// </summary>
    string Status { get; set; }

    /// <summary>
    /// Request creation time
    /// </summary>
    DateTime RequestedAt { get; set; }

    /// <summary>
    /// Processor ID (Admin)
    /// </summary>
    int? ProcessedBy { get; set; }

    /// <summary>
    /// Processing time
    /// </summary>
    DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Requester name (for quick display)
    /// </summary>
    string RequesterName { get; }

    /// <summary>
    /// Check if Can Approve
    /// </summary>
    bool CanApprove { get; }

    /// <summary>
    /// Check if Can Reject
    /// </summary>
    bool CanReject { get; }
}

/// <summary>
/// Enum defining the types of Requests in the system.
/// Easily extensible when there are new types.
/// </summary>
public enum RequestType
{
    UpgradeOwner = 1,
    HotelApproval = 2
    // Add new type here when needed
}

/// <summary>
/// Common statuses of Request - used for all types
/// </summary>
public static class RequestStatusConst
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string None = "None";

    /// <summary>
    /// Check if status is valid
    /// </summary>
    public static bool IsValid(string? status)
        => status == Pending || status == Approved || status == Rejected || status == Cancelled || status == None;

    /// <summary>
    /// Get all statuses
    /// </summary>
    public static List<string> GetAll() => new() { Pending, Approved, Rejected, Cancelled, None };

    /// <summary>
    /// Get filterable statuses (excluding None)
    /// </summary>
    public static List<string> GetFilterable() => new() { Pending, Approved, Rejected, Cancelled };
}

/// <summary>
/// Extension methods for RequestType enum
/// </summary>
public static class RequestTypeExtensions
{
    /// <summary>
    /// Get Vietnamese display name
    /// </summary>
    public static string GetDisplayName(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "Nâng cấp Owner",
        RequestType.HotelApproval => "Duyệt khách sạn",
        _ => type.ToString()
    };

    /// <summary>
    /// Get English display name
    /// </summary>
    public static string GetDisplayNameEn(this RequestType type) => type switch
    {
        RequestType.UpgradeOwner => "Upgrade Owner",
        RequestType.HotelApproval => "Hotel Approval",
        _ => type.ToString()
    };

    /// <summary>
    /// Parse from string to RequestType
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
/// Abstract base class for all Request DTOs.
/// Implements common properties, so concrete classes only need to define specific properties.
/// 
/// Design Pattern: Template Method + Polymorphism
/// - Common properties are implemented here
/// - Abstract properties force concrete classes to override
/// </summary>
public abstract class BaseRequestDTO : IRequestDTO
{
    // ==========================================
    // ABSTRACT PROPERTIES (Each request type is different)
    // ==========================================

    /// <summary>
    /// Request type - EACH concrete class MUST define
    /// </summary>
    public abstract RequestType Type { get; }

    /// <summary>
    /// Requester name - EACH concrete class MUST define
    /// (because field name could be different: UserName, OwnerName, HotelName...)
    /// </summary>
    public abstract string RequesterName { get; }

    // ==========================================
    // COMMON PROPERTIES (Shared across all types)
    // ==========================================

    public int RequestId { get; set; }
    public string Status { get; set; } = RequestStatusConst.Pending;
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }

    // ==========================================
    // COMPUTED PROPERTIES (Auto-calculated)
    // ==========================================

    /// <summary>
    /// Request type display name
    /// </summary>
    public string TypeDisplay => Type.GetDisplayName();

    /// <summary>
    /// Check if it Can Approve
    /// </summary>
    public bool CanApprove => Status == RequestStatusConst.Pending;

    /// <summary>
    /// Check if it Can Reject
    /// </summary>
    public bool CanReject => Status == RequestStatusConst.Pending;

    /// <summary>
    /// Check if request has been processed
    /// </summary>
    public bool IsProcessed => Status != RequestStatusConst.Pending;

    /// <summary>
    /// Check if request is successful
    /// </summary>
    public bool IsSuccessful => Status == RequestStatusConst.Approved;
}
