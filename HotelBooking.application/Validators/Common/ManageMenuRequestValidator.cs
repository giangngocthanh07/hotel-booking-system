using FluentValidation;

namespace HotelBooking.application.Validators.Common;

public class ManageMenuRequestValidator : AbstractValidator<ManageMenuRequest>
{
    public ManageMenuRequestValidator()
    {
        // Validate Enum value
        RuleFor(x => x.Module)
            .IsInEnum().WithMessage(MessageResponse.ManageMenu.INVALID_MODULE)
            .NotNull();
    }
}
