using FluentValidation;

namespace HotelBooking.application.Validators.UserManagement.Register
{
    public class RegisterCustomerValidator : AbstractValidator<RegisterCustomerDTO>
    {
        public RegisterCustomerValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_NAME);

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage(MessageResponse.Validation.EMPTY_NAME);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.INVALID_EMAIL)
                .EmailAddress().WithMessage(MessageResponse.UserManagement.Register.INVALID_EMAIL);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(MessageResponse.UserManagement.Register.EMPTY_PASSWORD)
                .MinimumLength(8).WithMessage(MessageResponse.UserManagement.Register.SHORT_PASSWORD)
                .Matches("[A-Z]").WithMessage(MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD)
                .Matches("[a-z]").WithMessage(MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD)
                .Matches("[0-9]").WithMessage(MessageResponse.UserManagement.Register.NUMBER_PASSWORD)
                .Matches("[^a-zA-Z0-9]").WithMessage(MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD);
        }
    }
}
