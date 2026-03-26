using System.Data;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;


namespace HotelBooking.application.Validators.RoomManagement
{
    public class RoomTypeCreateValidator : AbstractValidator<RoomTypeCreateDTO>
    {
        public RoomTypeCreateValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.HotelId)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_HOTEL_ID_INVALID);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_NAME_EMPTY)
                .MaximumLength(100).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_NAME_TOO_LONG);

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_DESCRIPTION_TOO_LONG);

            RuleFor(x => x.PricePerNight)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_PRICE_INVALID);

            RuleFor(x => x.AdultCapacity)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_ADULT_CAPACITY_INVALID);

            RuleFor(x => x.ChildCapacity)
                .GreaterThanOrEqualTo(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_CHILD_CAPACITY_INVALID);

            RuleFor(x => x.UnitTypeId)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_ID_INVALID);

            RuleFor(x => x.QualityId)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_ID_INVALID)
                .When(x => x.QualityId.HasValue);

            RuleFor(x => x.RoomViewId)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_ID_INVALID)
                .When(x => x.RoomViewId.HasValue);

            When(x => x.CanAddExtraBed == true, () =>
            {
                RuleFor(x => x.MaxExtraBeds)
                    .NotNull()
                    .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_MAX_EXTRA_BEDS_INVALID);
            });

            RuleFor(x => x.MaxExtraBeds)
                .Must(val => val == null || val == 0)
                .When(x => x.CanAddExtraBed == false)
                .WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_MAX_EXTRA_BEDS_MUST_BE_NULL_OR_ZERO);

            RuleFor(x => x.AreaSqm)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_AREA_INVALID)
                .When(x => x.AreaSqm.HasValue);

            RuleFor(x => x.TotalRooms)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_TOTAL_ROOMS_INVALID);

            RuleFor(x => x.BedTypes)
                .NotEmpty().WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPES_REQUIRED);

            RuleForEach(x => x.BedTypes).ChildRules(bed =>
            {
                bed.RuleFor(b => b.BedTypeId)
                    .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_ID_INVALID);
                bed.RuleFor(b => b.Quantity)
                    .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_QUANTITY_INVALID);
            });
        }
    }
}