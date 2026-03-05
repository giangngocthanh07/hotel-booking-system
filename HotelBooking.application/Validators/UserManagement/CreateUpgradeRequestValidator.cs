using FluentValidation;
using HotelBooking.application.DTOs.Request.UpgradeRequest;

namespace HotelBooking.application.Validators.UserManagement;

/// <summary>
/// Validator for create upgrade request
/// </summary>
public class CreateUpgradeRequestValidator : AbstractValidator<CreateUpgradeRequestDTO>
{
    public CreateUpgradeRequestValidator()
    {
        // 1. Validate Address - Address is required and must not exceed 500 characters
        RuleFor(x => x.Address)
            .NotEmpty().WithMessage(MessageResponse.RequestManagement.UpgradeRequest.ADDRESS_REQUIRED)
            .MaximumLength(500).WithMessage(MessageResponse.RequestManagement.UpgradeRequest.ADDRESS_TOO_LONG);

        // 2. Validate TaxCode - TaxCode is required and must be in the correct format
        RuleFor(x => x.TaxCode)
            .NotEmpty().WithMessage(MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_REQUIRED)
            .Matches(@"^\d{10}(\d{3})?$").WithMessage(MessageResponse.RequestManagement.UpgradeRequest.TAX_CODE_INVALID);
    }
}
