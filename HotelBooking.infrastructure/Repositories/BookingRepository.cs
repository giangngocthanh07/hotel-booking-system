using HotelBooking.infrastructure.Models;

public interface IBookingRepository : IRepository<Booking> { }
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(HotelBookingContext context) : base(context) { }
}
