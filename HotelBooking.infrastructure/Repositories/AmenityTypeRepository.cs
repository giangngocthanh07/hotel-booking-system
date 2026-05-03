using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IAmenityTypeRepository : IRepository<AmenityType>
{
    // Add custom methods for AmenityType here if needed
}

public class AmenityTypeRepository : Repository<AmenityType>, IAmenityTypeRepository
{
    public AmenityTypeRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}