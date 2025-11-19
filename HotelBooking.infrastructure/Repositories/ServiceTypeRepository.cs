using HotelBooking.infrastructure.Models;

public interface IServiceTypeRepository : IRepository<ServiceType>
{
    // Add custom methods for PolicyType here if needed
}

public class ServiceTypeRepository : Repository<ServiceType>, IServiceTypeRepository
{
    public ServiceTypeRepository(HotelBookingDBContext context) : base(context)
    {
    }
}