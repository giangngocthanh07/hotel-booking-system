using FluentValidation;
using HotelBooking.application.DTOs.Hotel;

public class RoomNameSuggestionValidator : AbstractValidator<RoomNameSuggestionRequest>
{
    public RoomNameSuggestionValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UnitTypeId)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_UNIT_TYPE_ID_INVALID);

        RuleFor(x => x.QualityId)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_QUALITY_ID_INVALID)
            .When(x => x.QualityId.HasValue);

        RuleFor(x => x.RoomViewId)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_ROOM_VIEW_ID_INVALID)
            .When(x => x.RoomViewId.HasValue);

        RuleFor(x => x.AdultCapacity)
            .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_ADULT_CAPACITY_INVALID);

        RuleFor(x => x.ChildrenCapacity)
        .GreaterThanOrEqualTo(0).WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_CHILDREN_CAPACITY_REQUIRED);

        When(x => x.CanAddExtraBeds == true, () =>
        {
            RuleFor(x => x.MaxExtraBeds)
                .NotNull().WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_MAX_EXTRA_BEDS_REQUIRED)
                .GreaterThan(0).WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_MAX_EXTRA_BEDS_INVALID);
        });

        RuleFor(x => x.MaxExtraBeds)
            .Must(val => val == null || val == 0)
            .When(x => x.CanAddExtraBeds == false)
            .WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_MAX_EXTRA_BEDS_MUST_BE_NULL_OR_ZERO);

        RuleFor(x => x.BedTypes)
            .NotEmpty().WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_BED_TYPES_REQUIRED);

        RuleForEach(x => x.BedTypes).ChildRules(bed =>
        {
            bed.RuleFor(b => b.BedTypeId)
                .GreaterThan(0)
                .WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_BED_TYPES_INVALID);

            bed.RuleFor(b => b.Quantity)
                .GreaterThan(0)
                .WithMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_BED_TYPES_QUANTITY_INVALID);
        });
    }
}