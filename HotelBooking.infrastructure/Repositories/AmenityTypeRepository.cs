using HotelBooking.infrastructure.Models;

public interface IAmenityTypeRepository : IRepository<AmenityType>
{
    // Add custom methods for AmenityType here if needed
}

public class AmenityTypeRepository : Repository<AmenityType>, IAmenityTypeRepository
{
    public AmenityTypeRepository(HotelBookingDBContext context) : base(context)
    {
    }
}