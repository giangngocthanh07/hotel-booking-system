using FluentValidation;

namespace HotelBooking.application.Validators.Common;

public class PagingRequestValidator : AbstractValidator<PagingRequest>
{
    public PagingRequestValidator()
    {
        // 1. PageIndex Validation
        RuleFor(x => x.PageIndex)
            .NotNull().WithMessage(MessageResponse.Pagination.MISSING_PAGE_INDEX)
            .GreaterThan(0).WithMessage(MessageResponse.Pagination.INVALID_PAGE_INDEX);

        // 2. PageSize Validation
        RuleFor(x => x.PageSize)
            .NotNull().WithMessage(MessageResponse.Pagination.MISSING_PAGE_SIZE)
            .GreaterThan(0).WithMessage(MessageResponse.Pagination.INVALID_PAGE_SIZE)
            .LessThanOrEqualTo(100).WithMessage(MessageResponse.Pagination.PAGE_SIZE_TOO_LARGE);
    }
}
