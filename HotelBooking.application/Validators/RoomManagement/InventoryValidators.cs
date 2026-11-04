using FluentValidation;
using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Validators.RoomManagement;

// Validate request shape before authorization or inventory queries run.
// Ownership and duplicate checks against persisted rooms belong to application services.
public class BatchAddRoomsValidator : AbstractValidator<BatchAddRoomsRequestDTO>
{
    public BatchAddRoomsValidator()
    {
        RuleFor(request => request.HotelId).GreaterThan(0);
        RuleFor(request => request.RoomTypeId).GreaterThan(0);
        RuleFor(request => request.RoomNumbers).NotEmpty();
        RuleForEach(request => request.RoomNumbers).NotEmpty();
    }
}

// A room-list query requires a positive RoomType identifier, even when no DTO is submitted.
public class RoomListQueryValidator : AbstractValidator<int>
{
    public RoomListQueryValidator() => RuleFor(roomTypeId => roomTypeId).GreaterThan(0);
}
