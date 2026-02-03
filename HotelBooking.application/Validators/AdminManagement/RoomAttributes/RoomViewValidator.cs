using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.RoomAttributes;

public class RoomViewCreateValidator : AbstractValidator<RoomViewCreateDTO>
{
    public RoomViewCreateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.RoomView.EMPTY_NAME)
        .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.LONG_NAME);
    }
}

public class RoomViewUpdateValidator : AbstractValidator<RoomViewUpdateDTO>
{
    public RoomViewUpdateValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(MessageResponse.AdminManagement.RoomAttribute.RoomView.EMPTY_NAME)
        .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.RoomAttribute.RoomView.LONG_NAME);
    }
}
