using FluentValidation;
using HotelBooking.application.DTOs.Request.UpgradeRequest;

namespace HotelBooking.application.Validators.RequestManagement.Admin;

public class RejectUpgradeRequestValidator : AbstractValidator<RejectUpgradeRequestDTO>
{
    public RejectUpgradeRequestValidator()
    {
        // FR-ONB-008: padding must not satisfy the minimum meaningful explanation length.
        RuleFor(request => request.Reason)
            .Must(reason => reason != null && reason.Trim().Length is >= 10 and <= 500)
            .WithMessage(MessageResponse.RequestManagement.UpgradeRequest.REJECTION_REASON_INVALID);
    }
}
