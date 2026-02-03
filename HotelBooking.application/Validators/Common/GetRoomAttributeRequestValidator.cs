using FluentValidation;
using HotelBooking.application.Validators.AdminManagement.RoomAttributes;

namespace HotelBooking.application.Validators.Common;

public class GetRoomAttributeRequestValidator : AbstractValidator<GetRoomAttributeRequest>
{
    public GetRoomAttributeRequestValidator()
    {
        // 1. Validate Enum
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.INVALID_TYPE);

        // 2. Validate Paging (TÁI SỬ DỤNG Validator đã viết ở bài trước)
        // Đây là cách FluentValidation lồng ghép nhau (Nested Validation)
        RuleFor(x => x.Paging)
            .NotNull()
            .SetValidator(new PagingRequestValidator());

        // 3. LOGIC ĐIỀU KIỆN (Conditional Validation)

        // TRƯỜNG HỢP A: Nếu là RoomQuality -> Bắt buộc phải có TypeId > 0
        When(x => x.Type == RoomAttributeType.RoomQuality, () =>
        {
            RuleFor(x => x.TypeId)
                .NotNull().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.MISSING_ROOM_QUALITY_TYPE)
                .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.INVALID_TYPE_ID);
        });

        // TRƯỜNG HỢP B: Các loại còn lại -> Bắt buộc TypeId phải NULL (Strict mode)
        When(x => x.Type != RoomAttributeType.RoomQuality, () =>
        {
            RuleFor(x => x.TypeId)
                .Null().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.UNSUPPORTED_TYPE_ID_FILTER);
        });
    }
}
