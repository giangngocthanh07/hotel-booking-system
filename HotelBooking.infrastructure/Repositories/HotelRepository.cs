using HotelBooking.infrastructure.Models;

public interface IHotelRepository : IRepository<Hotel> { }
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelBookingContext context) : base(context) { }
}
