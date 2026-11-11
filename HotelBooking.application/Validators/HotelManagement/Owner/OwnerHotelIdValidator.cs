using FluentValidation;

namespace HotelBooking.application.Validators.HotelManagement.Owner;

public class OwnerHotelIdValidator : AbstractValidator<int>
{
    public OwnerHotelIdValidator() => RuleFor(id => id).GreaterThan(0).WithMessage("A valid Hotel ID is required.");
}
