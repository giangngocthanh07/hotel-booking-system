using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.RequestManagement;

public sealed record HotelSearchSeed(int HotelId, int OwnerId, int RoomTypeId, int RoomId, string ProvinceName)
{
    public static async Task<HotelSearchSeed> CreateAsync(HotelRequestSqlFixture fixture)
    {
        var request = await fixture.SeedAsync();
        await using var approval = new HotelRequestRaceSession(fixture.Options);
        approval.Release();
        Assert.Equal(StatusCodeResponse.Success, (await approval.ExecuteAsync("Approved", request, request.AdminId)).StatusCode);
        await using var context = fixture.CreateContext();
        var hotel = await context.Hotels.SingleAsync(value => value.Name == request.Name);
        var roomType = CreateRoomType(hotel.Id);
        context.RoomTypes.Add(roomType);
        context.Reviews.Add(new Review { HotelId = hotel.Id, CustomerId = request.OwnerId, Rating = 5, IsDeleted = false });
        context.Reviews.Add(new Review { HotelId = hotel.Id, CustomerId = request.OwnerId, Rating = 4, IsDeleted = false });
        await context.SaveChangesAsync();
        var rooms = Enumerable.Range(1, 3).Select(index => new Room
            { RoomTypeId = roomType.Id, RoomNumber = index.ToString(), Status = "Available", IsDeleted = false }).ToList();
        context.Rooms.AddRange(rooms);
        await context.SaveChangesAsync();
        var province = await context.Provinces.SingleAsync(value => value.Id == hotel.ProvinceId);
        return new(hotel.Id, request.OwnerId, roomType.Id, rooms[0].Id, province.Name);
    }

    private static RoomType CreateRoomType(int hotelId) => new()
    {
        HotelId = hotelId, Name = "Standard room", PricePerNight = 123.45m, Capacity = 3,
        AdultCapacity = 2, ChildCapacity = 1, IsDeleted = false,
        UnitType = new UnitType { Name = $"Room{Guid.NewGuid():N}", IsDeleted = false },
    };

    public async Task ReserveOneRoomAsync(HotelBookingDBContext context)
    {
        // Preserve the deployed procedure's half-open overlap semantics for active bookings.
        context.Bookings.Add(new Booking
        {
            CustomerId = OwnerId, HotelId = HotelId, RoomTypeId = RoomTypeId, TotalPrice = 246.90m,
            CheckInDate = new DateOnly(2035, 1, 10), CheckOutDate = new DateOnly(2035, 1, 12),
            Status = "Confirmed", IsDeleted = false, BookingRooms = [new BookingRoom { RoomId = RoomId }],
        });
        await context.SaveChangesAsync();
    }
}
