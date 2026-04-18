
using System.Data;
using FluentValidation;

namespace HotelBooking.application.Validators.RequestManagement.Customer;

public class HotelRegistrationValidator : AbstractValidator<HotelRegistrationDTO>
{
    public HotelRegistrationValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_NAME)
            .MaximumLength(100).WithMessage(MessageResponse.Validation.LONG_NAME);

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_ADDRESS)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_ADDRESS);
    }
}