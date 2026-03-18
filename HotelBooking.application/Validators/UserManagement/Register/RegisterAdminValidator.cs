using FluentValidation;
using HotelBooking.application.DTOs.User.Register;

namespace HotelBooking.application.Validators.UserManagement.Register
{
    public class RegisterAdminValidator : AbstractValidator<RegisterAdminDTO>
    {
        public RegisterAdminValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.USERNAME_REQUIRED)
                .Length(8, 50).WithMessage(MessageResponse.UserManagement.Register.INVALID_USERNAME);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.FULLNAME_REQUIRED)
                .Length(8, 50).WithMessage(MessageResponse.UserManagement.Register.INVALID_FULLNAME);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.EMAIL_REQUIRED)
                .Length(8, 50).WithMessage(MessageResponse.UserManagement.Register.INVALID_EMAIL)
                .EmailAddress().WithMessage(MessageResponse.UserManagement.Register.INVALID_EMAIL_FORMAT);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.EMPTY_PHONE)
                .Length(10).WithMessage(MessageResponse.UserManagement.Register.INVALID_PHONE)
                .Matches("^[0-9]{10}$").WithMessage(MessageResponse.UserManagement.Register.INVALID_PHONE_FORMAT);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.EMPTY_PASSWORD)
                .MinimumLength(8).WithMessage(MessageResponse.UserManagement.Register.SHORT_PASSWORD)
                .Matches("[A-Z]").WithMessage(MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD)
                .Matches("[a-z]").WithMessage(MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD)
                .Matches("[0-9]").WithMessage(MessageResponse.UserManagement.Register.NUMBER_PASSWORD)
                .Matches("[^a-zA-Z0-9]").WithMessage(MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(MessageResponse.UserManagement.Register.PASSWORDS_DO_NOT_MATCH);
        }
    }
}
