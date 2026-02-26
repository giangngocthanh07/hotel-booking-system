using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement
{
    public class ManageMenuRequestValidator : AbstractValidator<ManageMenuRequest>
    {
        public ManageMenuRequestValidator()
        {
            // Kiểm tra xem Module gửi lên có hợp lệ không
            RuleFor(x => x.Module)
                .IsInEnum()
                .WithMessage(MessageResponse.Validation.INVALID_MODULE);
        }
    }
}