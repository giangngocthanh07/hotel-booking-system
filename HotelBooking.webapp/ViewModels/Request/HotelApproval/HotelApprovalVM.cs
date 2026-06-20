using HotelBooking.webapp.ViewModels.Request.Base;

namespace HotelBooking.webapp.ViewModels.Request.HotelApproval;

public class HotelRegistrationDetailVM : BaseRequestVM
{
    public override string RequesterName => Name;
    public override RequestType Type => RequestType.HotelApproval;

    public int? HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhoneNumber { get; set; } = string.Empty;
    public string OwnerAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PropertyTypeId { get; set; }
    public string PropertyTypeName { get; set; } = string.Empty;
    public int? StarRating { get; set; }
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CountryId { get; set; } = 4; // Default is 4 - Vietnam
    public int ProvinceId { get; set; }
    public int WardId { get; set; }
    public string ProvinceName { get; set; } = string.Empty;
    public string WardName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public string BusinessLicenseUrl { get; set; } = string.Empty;
    public string? AdminRemark { get; set; }

}

/// <summary>
/// Payload sent to POST v1/owner/hotel-registration.
/// Maps 1-to-1 with HotelRegistrationDTO on the backend.
/// </summary>
public class HotelRegistrationFormPayload
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PropertyTypeId { get; set; }
    public string PropertyTypeName { get; set; } = string.Empty;
    public int? StarRating { get; set; }
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CountryId { get; set; } = 4;
    public int ProvinceId { get; set; }
    public int WardId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public string BusinessLicenseUrl { get; set; } = string.Empty;
}
