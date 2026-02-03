using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.RoomAttributes;

// 1. Validate cho CREATE
public class BedTypeCreateValidator : AbstractValidator<BedTypeCreateDTO>
{
    public BedTypeCreateValidator()
    {
        // Check Tên
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME)
            .MaximumLength(50).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.LONG_NAME);

        // Check Sức chứa
        RuleFor(x => x.DefaultCapacity)
            .InclusiveBetween(1, 10).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);

        // Check Logic Kích thước (Chỉ check khi KHÔNG PHẢI là loại đa dạng)
        When(x => !x.IsVaryingSize, () =>
        {
            RuleFor(x => x.MinWidth)
                .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MIN_WIDTH);

            RuleFor(x => x.MaxWidth)
                .GreaterThanOrEqualTo(x => x.MinWidth)
                .WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MAX_WIDTH);
        });

        // Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

// 2. Validate cho UPDATE (Logic tương tự Create nhưng áp dụng cho DTO Update)
public class BedTypeUpdateValidator : AbstractValidator<BedTypeUpdateDTO>
{
    public BedTypeUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.LONG_NAME);

        RuleFor(x => x.DefaultCapacity)
            .InclusiveBetween(1, 10).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);

        When(x => !x.IsVaryingSize, () =>
        {
            RuleFor(x => x.MinWidth)
                .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MIN_WIDTH);

            RuleFor(x => x.MaxWidth)
                .GreaterThanOrEqualTo(x => x.MinWidth)
                .WithMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MAX_WIDTH);
        });

        // Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}
