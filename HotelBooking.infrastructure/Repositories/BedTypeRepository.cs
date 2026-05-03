using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IBedTypeRepository : IRepository<BedType>
{
    // Add custom methods for BedType here if needed
}

public class BedTypeRepository : Repository<BedType>, IBedTypeRepository
{
    public BedTypeRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}