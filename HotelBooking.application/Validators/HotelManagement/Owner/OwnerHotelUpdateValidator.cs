using FluentValidation;
using HotelBooking.application.DTOs.Hotel.Owner;

namespace HotelBooking.application.Validators.HotelManagement.Owner;

public class OwnerHotelUpdateValidator : AbstractValidator<OwnerHotelUpdateDTO>
{
    public OwnerHotelUpdateValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        DefineInformationRules();
        DefineLocationRules();
    }

    private void DefineInformationRules()
    {
        RuleFor(input => input.Name).NotEmpty().Must(name => name.Trim().Length is >= 6 and <= 50)
            .WithMessage("Hotel name must contain 6 to 50 characters.");
        RuleFor(input => input.Description).MaximumLength(500);
        RuleFor(input => input.Address).NotEmpty().Must(address => address.Trim().Length is >= 10 and <= 500)
            .WithMessage("Address must contain 10 to 500 characters.");
        RuleFor(input => input.PublicPhone).NotEmpty().Matches(@"^\d{10}$");
        RuleFor(input => input.PublicEmail).NotEmpty().MaximumLength(254).EmailAddress();
    }

    private void DefineLocationRules()
    {
        RuleFor(input => input.CountryId).GreaterThan(0);
        RuleFor(input => input.ProvinceId).GreaterThan(0);
        RuleFor(input => input.WardId).GreaterThan(0);
        // FR-CAT-002: zero is a supplied coordinate; neither NaN nor infinity can identify a location.
        RuleFor(input => input).Must(input => input.Latitude.HasValue == input.Longitude.HasValue)
            .WithMessage("Provide both Latitude and Longitude, or leave both empty.");
        RuleFor(input => input.Latitude).Must(value => !value.HasValue ||
            (double.IsFinite(value.Value) && value.Value is >= -90 and <= 90)).WithMessage("Latitude must be between -90 and 90.");
        RuleFor(input => input.Longitude).Must(value => !value.HasValue ||
            (double.IsFinite(value.Value) && value.Value is >= -180 and <= 180)).WithMessage("Longitude must be between -180 and 180.");
    }
}
