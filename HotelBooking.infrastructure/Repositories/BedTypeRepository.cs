using HotelBooking.infrastructure.Models;

public interface IBedTypeRepository : IRepository<BedType>
{
    // Add custom methods for BedType here if needed
}

public class BedTypeRepository : Repository<BedType>, IBedTypeRepository
{
    public BedTypeRepository(HotelBookingDBContext context) : base(context)
    {
    }
}