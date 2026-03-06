using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HotelBooking.webapp.ViewModels.Admin.Base;

namespace HotelBooking.webapp.ViewModels.Admin;

// ===========================================================================
// TYPE DEFINITIONS & ENUMS
// ===========================================================================

public class ServiceTypeVM : BaseAdminVM { }

public enum ServiceTypeEnum
{
    Standard = 1,
    AirportTransfer = 2,
}

// ===========================================================================
// POLYMORPHIC VIEW MODELS (Output - Display)
// ===========================================================================

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardVM), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportTransferVM), typeDiscriminator: "airportTransfer")]
public abstract class ServiceVM : BaseAdminVM
{
    [Range(1000, double.MaxValue, ErrorMessage = "Price must be at least 1,000!")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Service Type is required!")]
    public int TypeId { get; set; }
}

/// <summary>
/// Represents standard hotel services like Breakfast, Laundry, etc.
/// </summary>
public class ServiceStandardVM : ServiceVM
{
    [Required(ErrorMessage = "Unit is required!")]
    [MaxLength(20, ErrorMessage = "Unit name is too long (maximum 20 characters)!")]
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// Represents specialized Airport Transfer services with complex pricing.
/// </summary>
public class ServiceAirportTransferVM : ServiceVM
{
    // One-way pricing logic
    public bool IsOneWayPaid { get; set; }

    // Round-trip logic
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }

    [Range(1000, double.MaxValue, ErrorMessage = "Round-trip price must be at least 1,000!")]
    public decimal? RoundTripPrice { get; set; }

    // Surcharge/Night fee logic
    public bool HasNightFee { get; set; }

    [Range(1000, double.MaxValue, ErrorMessage = "Night fee must be at least 1,000!")]
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    // Vehicle Specifications
    [Range(1, 45, ErrorMessage = "Passenger capacity must be between 1 and 45!")]
    public int? MaxPassengers { get; set; }

    [Range(1, 45, ErrorMessage = "Luggage capacity must be between 1 and 45!")]
    public int? MaxLuggage { get; set; }
}

// ===========================================================================
// POLYMORPHIC CREATE MODELS (Input - POST)
// ===========================================================================

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardCreateVM), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportCreateVM), typeDiscriminator: "airport")]
public abstract class ServiceCreateVM : BaseCreateOrUpdateAdminVM
{
    [Range(1000, double.MaxValue, ErrorMessage = "Price must be at least 1,000!")]
    public decimal Price { get; set; } = 0;

    [JsonIgnore]
    public abstract int TargetTypeId { get; }
}

public class ServiceStandardCreateVM : ServiceCreateVM
{
    public override int TargetTypeId => (int)ServiceTypeEnum.Standard;

    [Required(ErrorMessage = "Please enter a unit (e.g., Person, Time, Room)")]
    [MaxLength(20, ErrorMessage = "Unit name cannot exceed 20 characters")]
    public string Unit { get; set; } = string.Empty;
}

public class ServiceAirportCreateVM : ServiceCreateVM
{
    public override int TargetTypeId => (int)ServiceTypeEnum.AirportTransfer;

    public bool IsOneWayPaid { get; set; }
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }

    [Range(1000, double.MaxValue, ErrorMessage = "Round-trip price must be at least 1,000!")]
    public decimal? RoundTripPrice { get; set; }

    public bool HasNightFee { get; set; }

    [Range(1000, double.MaxValue, ErrorMessage = "Night fee must be at least 1,000!")]
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    [Range(1, 45, ErrorMessage = "Passengers must be between 1-45")]
    public int? MaxPassengers { get; set; }

    [Range(1, 45, ErrorMessage = "Luggage count must be between 1-45")]
    public int? MaxLuggage { get; set; }
}

// ===========================================================================
// POLYMORPHIC UPDATE MODELS (Input - PUT)
// ===========================================================================

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardUpdateVM), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportUpdateVM), typeDiscriminator: "airport")]
public abstract class ServiceUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Range(1000, double.MaxValue, ErrorMessage = "Price must be at least 1,000!")]
    public decimal Price { get; set; } = 0;
}

public class ServiceStandardUpdateVM : ServiceUpdateVM
{
    [Required(ErrorMessage = "Please enter the unit")]
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
}

public class ServiceAirportUpdateVM : ServiceUpdateVM
{
    public bool IsOneWayPaid { get; set; }
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }

    [Range(1000, double.MaxValue, ErrorMessage = "Round-trip price must be at least 1,000!")]
    public decimal? RoundTripPrice { get; set; }

    public bool HasNightFee { get; set; }

    [Range(1000, double.MaxValue, ErrorMessage = "Night fee must be at least 1,000!")]
    public decimal? AdditionalFee { get; set; }

    [Required(ErrorMessage = "Start time is required")]
    public TimeOnly? AdditionalFeeStartTime { get; set; }

    [Required(ErrorMessage = "End time is required")]
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    [Range(1, 45, ErrorMessage = "Capacity must be between 1-45")]
    public int? MaxPassengers { get; set; }

    [Range(1, 45, ErrorMessage = "Luggage must be between 1-45")]
    public int? MaxLuggage { get; set; }
}

// ===========================================================================
// HELPER MODELS
// ===========================================================================

/// <summary>
/// Data structure for detailed Airport Transfer information.
/// </summary>
public class ServiceAirportAdditionalData
{
    public bool IsOneWayPaid { get; set; }
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }
    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
}

public class ManageServiceVM
{
    public List<ServiceTypeVM> ServiceTypes { get; set; } = new();
    public List<ServiceVM> Services { get; set; } = new();
    public int SelectedTypeId { get; set; }
    public string? SelectedTypeName { get; set; }
}