using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IBookingRepository : IRepository<Booking> { }
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
