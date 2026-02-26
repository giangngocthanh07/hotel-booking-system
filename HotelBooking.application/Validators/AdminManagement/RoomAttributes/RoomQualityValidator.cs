using FluentValidation;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Validators.AdminManagement.RoomAttributes;

public class RoomQualityCreateValidator : AbstractValidator<RoomQualityCreateDTO>
{
    public RoomQualityCreateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.EMPTY_NAME)
            .MaximumLength(50);

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 10).WithMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.INVALID_SORT_ORDER);

        // Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class RoomQualityUpdateValidator : AbstractValidator<RoomQualityUpdateDTO>
{
    public RoomQualityUpdateValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.EMPTY_NAME)
            .MaximumLength(50);

        RuleFor(x => x.SortOrder)
            .InclusiveBetween(0, 10).WithMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.INVALID_SORT_ORDER);

        // Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}
