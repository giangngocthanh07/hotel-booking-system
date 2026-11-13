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
            .WithMessage("Please choose a genuine JPG, JPEG or PNG image. Renaming another file to .jpg or .png does not convert it into an image.");
        RuleFor(file => file.Size).InclusiveBetween(1, HotelImageFileRules.MaximumSize).WithMessage("Choose a photo that is not empty and no larger than 10 MB.");
        RuleFor(file => file.Content).NotNull().Must(content => content.CanRead).WithMessage("This photo could not be read. Choose the original JPG, JPEG or PNG photo and try again.");
    }
}
