using FluentValidation;

namespace HotelBooking.application.Validators.Common;

public class ManageMenuRequestValidator : AbstractValidator<ManageMenuRequest>
{
    public ManageMenuRequestValidator()
    {
        // Kiểm tra giá trị Enum có hợp lệ không
        // (FluentValidation có sẵn hàm IsInEnum, cực tiện)
        RuleFor(x => x.Module)
            .IsInEnum().WithMessage(MessageResponse.ManageMenu.INVALID_MODULE)
            .NotNull();
    }
}
