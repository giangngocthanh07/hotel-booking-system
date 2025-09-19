using HotelBooking.infrastructure.Models;

public interface IBookingRoomRepository : IRepository<BookingRoom>
{
    // Add custom methods for BookingRoom here if needed
}

public class BookingRoomRepository : Repository<BookingRoom>, IBookingRoomRepository
{
    public BookingRoomRepository(HotelBookingContext context) : base(context)
    {
    }
}