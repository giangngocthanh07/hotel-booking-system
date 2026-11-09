using HotelBooking.application.Services.Helpers.Media.BusinessLicenses;
using FluentValidation;

namespace HotelBooking.application.Validators.Media.BusinessLicenses;

public class BusinessLicenseUploadValidator : AbstractValidator<UploadFileDTO>
{
    public BusinessLicenseUploadValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(file => file.FileName).NotEmpty()
            .MaximumLength(255).WithMessage("Business license filename must contain no more than 255 characters.")
            .Must(name => BusinessLicenseFileRules.ExpectedMime(name) != null)
            .WithMessage("Business license must be a PDF, JPEG or PNG file.");
        RuleFor(file => file.ContentType).NotEmpty()
            .Must((file, mime) => string.Equals(mime, BusinessLicenseFileRules.ExpectedMime(file.FileName), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Business license MIME type must match its file extension.");
        RuleFor(file => file.Size).InclusiveBetween(1, BusinessLicenseFileRules.MaximumSize)
            .WithMessage("Business license must contain data and be no larger than 10 MiB.");
        RuleFor(file => file.Content).NotNull().Must(content => content.CanRead)
            .WithMessage("Business license content must be readable.");
    }
}
