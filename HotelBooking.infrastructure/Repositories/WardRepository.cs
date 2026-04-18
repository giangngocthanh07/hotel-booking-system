using HotelBooking.infrastructure.Models;

public interface IWardRepository : IRepository<Ward>
{
    // Add custom methods for Ward here if needed
}

public class WardRepository : Repository<Ward>, IWardRepository
{
    public WardRepository(HotelBookingDBContext context) : base(context)
    {
    }
}