using FluentValidation;

namespace HotelBooking.application.Validators.UserManagement;

/// <summary>
/// Validator cho yêu cầu tạo upgrade request
/// </summary>
public class CreateUpgradeRequestValidator : AbstractValidator<CreateUpgradeRequestDTO>
{
    public CreateUpgradeRequestValidator()
    {
        // 1. Validate Address - Địa chỉ không được trống
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage(MessageResponse.RequestManagement.UpgradeRequest.ADDRESS_REQUIRED)
            .MaximumLength(500).WithMessage(MessageResponse.RequestManagement.UpgradeRequest.ADDRESS_TOO_LONG);

        // 2. Validate TaxCode - Mã số thuế không được trống và phải đúng format
        RuleFor(x => x.TaxCode)
            .NotEmpty().WithMessage(MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_REQUIRED)
            .Matches(@"^\d{10}(\d{3})?$").WithMessage(MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_INVALID);
    }
}
