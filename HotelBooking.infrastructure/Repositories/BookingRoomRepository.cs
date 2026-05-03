using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IBookingRoomRepository : IRepository<BookingRoom>
{
    // Add custom methods for BookingRoom here if needed
}

public class BookingRoomRepository : Repository<BookingRoom>, IBookingRoomRepository
{
    public BookingRoomRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}