using FluentValidation;
using HotelBooking.application.DTOs.User.Login;

namespace HotelBooking.application.Validators.UserManagement.Login
{
    /// <summary>
    /// Validator for login - only validate input
    /// Logic authentication username/password is in UserService
    /// </summary>
    public class LoginValidator : AbstractValidator<LoginUserDTO>
    {
        public LoginValidator()
        {
            // Validate UsernameOrEmail - only check if it's not empty & has a reasonable length
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Login.EMPTY_USERNAME_OR_EMAIL)
                .MaximumLength(255).WithMessage(MessageResponse.UserManagement.Login.MAX_LENGTH_USERNAME_OR_EMAIL);

            // Validate Password - only check if it's not empty & has a reasonable length
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.EMPTY_PASSWORD)
                .MaximumLength(100).WithMessage(MessageResponse.UserManagement.Login.MAX_LENGTH_PASSWORD);
        }
    }
}
