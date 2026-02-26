using FluentValidation;

namespace HotelBooking.application.Validators.AdminManagement.RoomAttributes
{
    public class GetRoomAttributeRequestValidator : AbstractValidator<GetRoomAttributeRequest>
    {
        public GetRoomAttributeRequestValidator(IValidator<PagingRequest> pagingValidator)
        {
            // 1. Kiểm tra Enum Type: Tránh trường hợp gửi số không nằm trong danh sách (1-4)
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.INVALID_TYPE);

            // 2. Kiểm tra TypeId: Chỉ validate nếu TypeId có giá trị (vì là int?)
            RuleFor(x => x.TypeId)
                .GreaterThan(0).When(x => x.TypeId.HasValue)
                .WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.INVALID_TYPE_ID);

            // 3. QUAN TRỌNG: Validate lồng nhau (Nested Validation)
            // Nó sẽ tự động lấy kết quả từ PagingRequestValidator để báo lỗi
            RuleFor(x => x.Paging)
                .NotNull().WithMessage(MessageResponse.AdminManagement.RoomAttribute.Request.PAGINATION_REQUIRED)
                .SetValidator(pagingValidator); 
        }
    }
}