using FluentValidation;
using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Services.Helpers.Media.HotelImages;

namespace HotelBooking.application.Validators.HotelManagement.Owner.Images;

public class OwnerHotelImageUploadValidator : AbstractValidator<OwnerHotelImageFileDTO>
{
    public OwnerHotelImageUploadValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(file => file.FileName).NotEmpty().MaximumLength(255)
            .Must(name => HotelImageFileRules.ExpectedMime(name) != null).WithMessage("Hotel images must be JPEG or PNG files.");
        RuleFor(file => file.ContentType).NotEmpty()
            .Must((file, mime) => string.Equals(mime, HotelImageFileRules.ExpectedMime(file.FileName), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Image MIME type must match the file extension.");
        RuleFor(file => file.Size).InclusiveBetween(1, HotelImageFileRules.MaximumSize).WithMessage("Hotel images must contain data and be no larger than 10 MiB.");
        RuleFor(file => file.Content).NotNull().Must(content => content.CanRead).WithMessage("Image content must be readable.");
    }
}
