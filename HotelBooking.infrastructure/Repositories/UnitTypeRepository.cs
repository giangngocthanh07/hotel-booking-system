using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IUnitTypeRepository : IRepository<UnitType>
{
    // Add custom methods for UnitType here if needed
}

public class UnitTypeRepository : Repository<UnitType>, IUnitTypeRepository
{
    public UnitTypeRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}