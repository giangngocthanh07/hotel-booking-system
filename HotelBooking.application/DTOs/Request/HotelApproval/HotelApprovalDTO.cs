
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.Base;

namespace HotelBooking.application.DTOs.Request.HotelApproval;

public class HotelRegistrationDetailDTO : BaseRequestDTO
{
    public override RequestType Type { get; } = RequestType.HotelApproval;
    public override string RequesterName => Name;
    public int? HotelId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string BusinessLicenseUrl { get; set; } = string.Empty;

    public int OwnerId { get; set; }
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhoneNumber { get; set; } = string.Empty;
    public string OwnerAddress { get; set; } = string.Empty;

    public string? AdminRemark { get; set; }

    public string? Description { get; set; }
    public int PropertyTypeId { get; set; }
    public string PropertyTypeName { get; set; } = string.Empty;
    public int? StarRating { get; set; }
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public int CountryId { get; set; } = 4; // Default is 4 - Vietnam
    public int ProvinceId { get; set; }
    public int WardId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public string WardName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

/// <summary>
/// JSON blob stored in HotelApprovalRequest.Additional.
/// Holds all hotel fields that are NOT direct columns on the table.
/// Used for both serialization (Owner submit) and deserialization (Admin/Owner read).
/// </summary>
public class HotelAdditionalInfo
{
    public string? Description { get; set; }

    public int? StarRating { get; set; }
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;

    public PropertyTypeDTO PropType { get; set; } = new();
    public CountryDTO Country { get; set; } = new();
    public ProvinceDTO Province { get; set; } = new();
    public WardDTO Ward { get; set; } = new();

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class HotelEntityAdditionalData // Only for Hotel Entity
{
    public int? StarRating { get; set; }
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public string TaxCode { get; set; } = string.Empty;
    public string BusinessLicenseUrl { get; set; } = string.Empty;
}


