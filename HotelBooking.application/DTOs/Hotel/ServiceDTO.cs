using System.Text.Json.Serialization;

public class ServiceTypeDTO : BaseAdminDTO
{
}

public enum ServiceTypeEnum
{
    Standard = 1,
    AirportTransfer = 2,
}

// Indicate this is a polymorphic class
[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
// Declare children and set identifiers (discriminators) for them
[JsonDerivedType(typeof(ServiceStandardDTO), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportTransferDTO), typeDiscriminator: "airportTransfer")]
public abstract class ServiceDTO : BaseAdminDTO
{
    public decimal Price { get; set; } = 0;
    public int TypeId { get; set; }
}

public class ServiceStandardDTO : ServiceDTO
{
    // Add to Additional field in DB
    public string Unit { get; set; } = string.Empty;

}

public class ServiceAirportTransferDTO : ServiceDTO
{

    // Question: Is one-way paid?
    public bool IsOneWayPaid { get; set; }

    // Question: Is round trip supported?
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    // Question: Is there a night fee?
    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    // Basic parameters
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }

}

// DTO to display airport transfer service with specific fields
public class ServiceAirportAdditionalData
{
    // Question: Is one-way paid?
    public bool IsOneWayPaid { get; set; }

    // Question: Is round trip supported?
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    // Question: Is there a night fee?
    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public string? AdditionalFeeStartTime { get; set; }
    public string? AdditionalFeeEndTime { get; set; }

    // Basic parameters
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardCreateDTO), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportCreateDTO), typeDiscriminator: "airport")]
// Add/Edit service DTO
public abstract class ServiceCreateDTO : BaseCreateOrUpdateAdminDTO
{
    public decimal Price { get; set; } = 0;

    // Virtual getter for backend to know what type is being created
    [JsonIgnore]
    public abstract int TargetTypeId { get; }
}

public class ServiceStandardCreateDTO : ServiceCreateDTO
{
    public override int TargetTypeId => (int)ServiceTypeEnum.Standard;
    public string Unit { get; set; } = string.Empty;
}

public class ServiceAirportCreateDTO : ServiceCreateDTO
{
    public override int TargetTypeId => (int)ServiceTypeEnum.AirportTransfer;

    // Question: Is one-way paid?
    public bool IsOneWayPaid { get; set; }

    // Question: Is round trip supported?
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    // Question: Is there a night fee?
    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }

    // Basic parameters
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "discriminator")]
[JsonDerivedType(typeof(ServiceStandardUpdateDTO), typeDiscriminator: "standard")]
[JsonDerivedType(typeof(ServiceAirportUpdateDTO), typeDiscriminator: "airport")]
public abstract class ServiceUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public decimal Price { get; set; }

    // No TargetTypeId -> Cannot change service type
}

// 1. Update Standard
public class ServiceStandardUpdateDTO : ServiceUpdateDTO
{
    public string Unit { get; set; } = string.Empty;
}

// 2. Update Airport
public class ServiceAirportUpdateDTO : ServiceUpdateDTO
{
    public bool IsOneWayPaid { get; set; }
    public bool HasRoundTrip { get; set; }
    public bool IsRoundTripPaid { get; set; }
    public decimal? RoundTripPrice { get; set; }

    public bool HasNightFee { get; set; }
    public decimal? AdditionalFee { get; set; }
    public TimeOnly? AdditionalFeeStartTime { get; set; }
    public TimeOnly? AdditionalFeeEndTime { get; set; }
    // Basic parameters
    public int? MaxPassengers { get; set; }
    public int? MaxLuggage { get; set; }
}



