using FluentValidation;

namespace HotelBooking.application.Validators.UserManagement.Login
{
    public class LoginValidator : AbstractValidator<LoginUserDTO>
    {
        public LoginValidator()
        {
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage(MessageResponse.Common.BAD_REQUEST);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.EMPTY_PASSWORD);
        }
    }
}
