using FluentValidation;
using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Validators.RoomManagement;

public class HotelSearchValidator : AbstractValidator<HotelSearchRequestDTO>
{
    public HotelSearchValidator()
    {
        RuleFor(x => x.CityName).NotEmpty().WithMessage("Vui lòng nhập tên thành phố.");
        
        RuleFor(x => x.CheckIn)
            .NotEmpty().WithMessage("Vui lòng chọn ngày nhận phòng.")
            .Must(x => x >= DateTime.Today).WithMessage("Ngày nhận phòng không được ở quá khứ.");

        RuleFor(x => x.CheckOut)
            .NotEmpty().WithMessage("Vui lòng chọn ngày trả phòng.")
            .GreaterThan(x => x.CheckIn).WithMessage("Ngày trả phòng phải sau ngày nhận phòng.");

        RuleFor(x => x.Adults).GreaterThan(0).WithMessage("Số lượng người lớn ít nhất là 1.");
        RuleFor(x => x.Rooms).GreaterThan(0).WithMessage("Số lượng phòng ít nhất là 1.");
    }
}
