using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.Amenities;

public class AmenityCreateValidator : AbstractValidator<AmenityCreateDTO>
{
    public AmenityCreateValidator()
    {
        // 1. Validate Name
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Amenity.EMPTY_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Amenity.LONG_NAME);

        // 2a. Validate TypeId must not be empty (REQUIRED for creation)
        RuleFor(x => x.TypeId)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Amenity.EMPTY_TYPE);

        // 2b. Validate TypeId must be greater than 0
        RuleFor(x => x.TypeId)
            .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.Amenity.GREATER_THAN_ZERO);

        // 3. Validate TypeId validity (Check if it exists in the Enum)
        RuleFor(x => x.TypeId)
            .Must(typeId => Enum.IsDefined(typeof(AmenityTypeEnum), typeId))
            .WithMessage(MessageResponse.AdminManagement.Amenity.INVALID_TYPE);

        // 4. Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class AmenityUpdateValidator : AbstractValidator<AmenityUpdateDTO>
{
    public AmenityUpdateValidator()
    {
        // 1. Validate Name (Same logic as Create for consistency)
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Amenity.EMPTY_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Amenity.LONG_NAME);

        // 2. Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        // [IMPORTANT]: TypeId is not validated here as it's typically immutable during update
    }
}