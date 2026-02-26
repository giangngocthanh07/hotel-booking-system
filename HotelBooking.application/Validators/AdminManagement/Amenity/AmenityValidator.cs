using FluentValidation;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Validators.AdminManagement.Amenity;

public class AmenityCreateValidator : AbstractValidator<AmenityCreateDTO>
{
    public AmenityCreateValidator()
    {
        // 1. Validate Tên (Giống cũ)
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Amenity.EMPTY_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Amenity.LONG_NAME);

        // 2a. Validate TypeId không được rỗng (BẮT BUỘC vì đang tạo mới)
        RuleFor(x => x.TypeId)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Amenity.EMPTY_TYPE);

        // 2b. Validate TypeId lớn hơn 0 (BẮT BUỘC vì đang tạo mới)
        RuleFor(x => x.TypeId)
            .GreaterThan(0).WithMessage(MessageResponse.AdminManagement.Amenity.GREATER_THAN_ZERO);

        // 3. Validate TypeId có hợp lệ không (Có tồn tại trong enum không)
        RuleFor(x => x.TypeId)
            .Must(typeId => Enum.IsDefined(typeof(AmenityTypeEnum), typeId))
            .WithMessage(MessageResponse.AdminManagement.Amenity.INVALID_TYPE);

        // 4. Validate Description (Nếu cần - Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class AmenityUpdateValidator : AbstractValidator<AmenityUpdateDTO>
{
    public AmenityUpdateValidator()
    {
        // 1. Validate Tên (Copy logic từ Create sang để đảm bảo nhất quán)
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(MessageResponse.AdminManagement.Amenity.EMPTY_NAME)
            .MaximumLength(20).WithMessage(MessageResponse.AdminManagement.Amenity.LONG_NAME);

        // 2. Validate Description (Optional)
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage(MessageResponse.Validation.LONG_DESCRIPTION);

        // [QUAN TRỌNG]: Không có RuleFor(TypeId) ở đây
    }
}
