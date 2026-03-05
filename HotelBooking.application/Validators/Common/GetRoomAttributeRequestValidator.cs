using FluentValidation;

namespace HotelBooking.application.Validators.Common;

public class GetRoomAttributeRequestValidator : AbstractValidator<GetRoomAttributeRequest>
{
    public GetRoomAttributeRequestValidator()
    {
        // 1. Enum Validation
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.INVALID_TYPE);

        // 2. Paging Validation (Reusing existing PagingRequestValidator)
        // This implements FluentValidation's Nested Validation
        RuleFor(x => x.Paging)
            .NotNull()
            .SetValidator(new PagingRequestValidator());

        // 3. CONDITIONAL VALIDATION LOGIC

        // CASE A: If the attribute is RoomQuality -> TypeId is REQUIRED and must be > 0
        When(x => x.Type == RoomAttributeType.RoomQuality, () =>
        {
            RuleFor(x => x.TypeId)
                .NotNull().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.MISSING_ROOM_QUALITY_TYPE)
                .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.INVALID_TYPE_ID);
        });

        // CASE B: For other types -> TypeId must be NULL (Strict mode)
        When(x => x.Type != RoomAttributeType.RoomQuality, () =>
        {
            RuleFor(x => x.TypeId)
                .Null().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.UNSUPPORTED_TYPE_ID_FILTER);
        });
    }
}