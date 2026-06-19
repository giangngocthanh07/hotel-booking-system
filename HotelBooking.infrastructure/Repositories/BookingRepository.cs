using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

using Microsoft.EntityFrameworkCore;

public interface IBookingRepository : IRepository<Booking> 
{
    Task<List<Booking>> GetBookingsByHotelsAsync(List<int> hotelIds);
}
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }

    public async Task<List<Booking>> GetBookingsByHotelsAsync(List<int> hotelIds)
    {
        return await _dbSet
            .Include(b => b.Customer)
            .Include(b => b.RoomType)
            .Where(b => hotelIds.Contains(b.HotelId) && b.Status != "Cancelled")
            .ToListAsync(_cancellationToken);
    }
}
