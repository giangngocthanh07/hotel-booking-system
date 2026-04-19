
using HotelBooking.application.DTOs.Request.Base;

namespace HotelBooking.application.DTOs.Request.HotelApproval;

public class HotelRegistrationDetailDTO : BaseRequestDTO
{
    public override RequestType Type { get; } = RequestType.HotelApproval;
    public override string RequesterName => Name;
    public int HotelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OwnerId { get; set; }
    public string OwnerFullName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPhoneNumber { get; set; } = string.Empty;
    public string OwnerAddress { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PropertyTypeId { get; set; }
    public int? StarRating { get; set; }
    public string PublicPhone { get; set; } = string.Empty;
    public string PublicEmail { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int CountryId { get; set; } = 4; // Default is 4 - Vietnam
    public int ProvinceId { get; set; }
    public int WardId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public string BusinessLicenseUrl { get; set; } = string.Empty;
}


