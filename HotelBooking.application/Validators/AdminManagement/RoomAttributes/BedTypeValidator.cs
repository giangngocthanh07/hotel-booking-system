using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.RoomAttributes;

// 1. Validator for CREATE
public class BedTypeCreateValidator : AbstractValidator<BedTypeCreateDTO>
{
    public BedTypeCreateValidator()
    {
        // Name Validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.LONG_NAME);

        // Capacity Validation
        RuleFor(x => x.DefaultCapacity)
            .InclusiveBetween(1, 10).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);

        // Size Logic Validation (Only applicable if the bed is NOT a varying size type)
        When(x => !x.IsVaryingSize, () =>
        {
            RuleFor(x => x.MinWidth)
                .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MIN_WIDTH);

            RuleFor(x => x.MaxWidth)
                .GreaterThanOrEqualTo(x => x.MinWidth)
                .WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MAX_WIDTH);
        });

        // Description Validation (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

// 2. Validator for UPDATE (Similar logic to Create but applied to UpdateDTO)
public class BedTypeUpdateValidator : AbstractValidator<BedTypeUpdateDTO>
{
    public BedTypeUpdateValidator()
    {
        // Name Validation
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.LONG_NAME);

        // Capacity Validation
        RuleFor(x => x.DefaultCapacity)
            .InclusiveBetween(1, 10).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);

        // Size Logic Validation
        When(x => !x.IsVaryingSize, () =>
        {
            RuleFor(x => x.MinWidth)
                .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MIN_WIDTH);

            RuleFor(x => x.MaxWidth)
                .GreaterThanOrEqualTo(x => x.MinWidth)
                .WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MAX_WIDTH);
        });

        // Description Validation (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}