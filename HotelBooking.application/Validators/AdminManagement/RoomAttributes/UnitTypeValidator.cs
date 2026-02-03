using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.RoomAttributes;

public class UnitTypeCreateValidator : AbstractValidator<UnitTypeCreateDTO>
{
    public UnitTypeCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.LONG_NAME);

        // IsEntirePlace là bool nên không cần validate (true/false đều hợp lệ)

        // Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class UnitTypeUpdateValidator : AbstractValidator<UnitTypeUpdateDTO>
{
    public UnitTypeUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.LONG_NAME);

        // Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}
