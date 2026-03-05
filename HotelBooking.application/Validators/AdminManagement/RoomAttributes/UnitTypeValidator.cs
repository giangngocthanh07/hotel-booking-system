using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.RoomAttributes;

public class UnitTypeCreateValidator : AbstractValidator<UnitTypeCreateDTO>
{
    public UnitTypeCreateValidator()
    {
        // Name Validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.LONG_NAME);

        // Note: IsEntirePlace is a boolean, so explicit validation is not required 
        // as both true and false are inherently valid values.

        // Description Validation (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class UnitTypeUpdateValidator : AbstractValidator<UnitTypeUpdateDTO>
{
    public UnitTypeUpdateValidator()
    {
        // Name Validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.LONG_NAME);

        // Description Validation (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}