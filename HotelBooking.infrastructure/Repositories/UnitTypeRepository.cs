using HotelBooking.infrastructure.Models;

public interface IUnitTypeRepository : IRepository<UnitType>
{
    // Add custom methods for UnitType here if needed
}

public class UnitTypeRepository : Repository<UnitType>, IUnitTypeRepository
{
    public UnitTypeRepository(HotelBookingDBContext context) : base(context)
    {
    }
}