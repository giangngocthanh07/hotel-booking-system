using HotelBooking.application.Services.Helpers.Media.BusinessLicenses;
using HotelBooking.application.DTOs.Media.BusinessLicenses;
using FluentValidation;

namespace HotelBooking.application.Validators.Media.BusinessLicenses;

public class BusinessLicenseDownloadValidator : AbstractValidator<BusinessLicenseDownloadSource>
{
    public BusinessLicenseDownloadValidator()
    {
        RuleFor(source => source.PublicId).NotEmpty().MaximumLength(512);
        RuleFor(source => source.ResourceType).Equal("raw");
        RuleFor(source => source.DeliveryType).Equal("private");
        RuleFor(source => source.SizeBytes).InclusiveBetween(1, BusinessLicenseFileRules.MaximumSize);
        RuleFor(source => source.ContentType).NotEmpty().Must((source, mime) =>
            BusinessLicenseFileRules.ExpectedMime(source.PublicId) is { } expected && expected == mime);
    }
}
