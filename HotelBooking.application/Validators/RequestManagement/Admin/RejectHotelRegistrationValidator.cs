using FluentValidation;
using HotelBooking.application.DTOs.Request.HotelApproval;

namespace HotelBooking.application.Validators.RequestManagement.Admin;

public class RejectHotelRegistrationValidator : AbstractValidator<RejectHotelRegistrationDTO>
{
    public RejectHotelRegistrationValidator()
    {
        // FR-ONB-017 requires a usable explanation of 10–500 characters.
        // Count the trimmed value that will be saved so padding cannot satisfy the minimum.
        RuleFor(request => request.Reason)
            .Must(reason => reason != null && reason.Trim().Length is >= 10 and <= 500)
            .WithMessage(MessageResponse.RequestManagement.HotelApproval.REJECTION_REASON_INVALID);
    }
}
