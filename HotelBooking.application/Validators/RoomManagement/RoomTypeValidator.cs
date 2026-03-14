using System.Data;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Validators.RoomManagement
{
    public class RoomTypeValidator : AbstractValidator<RoomTypeCreateDTO>
    {
        public RoomTypeValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_NAME_EMPTY)
                .MaximumLength(100).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_NAME_TOO_LONG);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_DESCRIPTION_TOO_LONG);

            RuleFor(x => x.PricePerNight)
                .GreaterThanOrEqualTo(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_PRICE_INVALID);

            RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_CAPACITY_INVALID);

            RuleFor(x => x.AdultCapacity)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_ADULT_CAPACITY_INVALID);

            RuleFor(x => x.ChildCapacity)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_CHILD_CAPACITY_INVALID);

            RuleFor(x => x.UnitTypeId)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_ID_INVALID);

            RuleFor(x => x.QualityId)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_ID_INVALID);

            RuleFor(x => x.RoomViewId)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_ID_INVALID);

            RuleFor(x => x.MaxExtraBeds)
            .GreaterThanOrEqualTo(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_MAX_EXTRA_BEDS_INVALID);

            RuleFor(x => x.AreaSqm)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_AREA_INVALID);
            
            RuleFor(x => x.TotalRooms)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_TOTAL_ROOMS_INVALID);
        }
    }
}