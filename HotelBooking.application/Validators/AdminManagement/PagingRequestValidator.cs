using FluentValidation;
using HotelBooking.application.Validators.Common;

public class PagingRequestValidator : AbstractValidator<PagingRequest>
{
    public PagingRequestValidator()
    {
        RuleFor(x => x.PageIndex)
            .GreaterThan(0)
            .WithMessage(MessageResponse.Pagination.INVALID_PAGE_INDEX);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage(MessageResponse.Pagination.INVALID_PAGE_SIZE)
            .LessThanOrEqualTo(100)
            .WithMessage(MessageResponse.Pagination.PAGE_SIZE_TOO_LARGE);
    }
} 