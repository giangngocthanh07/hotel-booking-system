using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IAmenityRepository : IRepository<Amenity> { }
public class AmenityRepository : Repository<Amenity>, IAmenityRepository
{
    public AmenityRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
