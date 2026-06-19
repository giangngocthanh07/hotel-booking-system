using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

using Microsoft.EntityFrameworkCore;

public interface IRoomRepository : IRepository<Room> 
{
    Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime checkIn, DateTime checkOut, int count);
    Task<List<Room>> GetRoomsByHotelsAsync(List<int> hotelIds);
}
public class RoomRepository : Repository<Room>, IRoomRepository
{
    public RoomRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }

    public async Task<List<Room>> GetAvailableRoomsAsync(int roomTypeId, DateTime checkIn, DateTime checkOut, int count)
    {
        var checkInDate = DateOnly.FromDateTime(checkIn);
        var checkOutDate = DateOnly.FromDateTime(checkOut);

        // Subquery: Get all room IDs that are already booked for the given dates
        var bookedRoomIds = _context.Set<BookingRoom>()
            .Where(br => br.Booking.Status != "Cancelled" &&
                         br.Booking.CheckInDate < checkOutDate &&
                         br.Booking.CheckOutDate > checkInDate)
            .Select(br => br.RoomId);

        // Filter rooms in the RoomType that are not in the bookedRoomIds list
        return await _dbSet
            .Where(r => r.RoomTypeId == roomTypeId && 
                        r.Status == "Active" && 
                        !bookedRoomIds.Contains(r.Id))
            .Take(count)
            .ToListAsync(_cancellationToken);
    }

    public async Task<List<Room>> GetRoomsByHotelsAsync(List<int> hotelIds)
    {
        return await _dbSet
            .Where(r => hotelIds.Contains(r.RoomType.HotelId) && r.IsDeleted != true)
            .ToListAsync(_cancellationToken);
    }
}
