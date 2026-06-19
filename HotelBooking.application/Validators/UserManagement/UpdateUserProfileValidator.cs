using FluentValidation;
using HotelBooking.application.DTOs.User;

namespace HotelBooking.application.Validators.UserManagement;

public class UpdateUserProfileValidator : AbstractValidator<UpdateUserProfileDTO>
{
    public UpdateUserProfileValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(100).WithMessage("Họ tên không quá 100 ký tự.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10,11}$").WithMessage("Số điện thoại không hợp lệ (10-11 chữ số).")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Ngày sinh phải ở quá khứ.")
            .When(x => x.DateOfBirth.HasValue);
    }
}
