using FluentValidation;

namespace HotelBooking.application.Validators.UserManagement.Login
{
    /// <summary>
    /// Validator cho đăng nhập - chỉ validate input cơ bản
    /// Logic xác thực username/password đúng/sai nằm ở UserService
    /// </summary>
    public class LoginValidator : AbstractValidator<LoginUserDTO>
    {
        public LoginValidator()
        {
            // Validate UsernameOrEmail - chỉ check có nhập hay không & độ dài hợp lý
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Login.EMPTY_USERNAME_OR_EMAIL)
                .MaximumLength(255).WithMessage(MessageResponse.UserManagement.Login.MAX_LENGTH_USERNAME_OR_EMAIL);

            // Validate Password - chỉ check có nhập hay không & độ dài hợp lý
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.EMPTY_PASSWORD)
                .MaximumLength(100).WithMessage(MessageResponse.UserManagement.Login.MAX_LENGTH_PASSWORD);
        }
    }
}
