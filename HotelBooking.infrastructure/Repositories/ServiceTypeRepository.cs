using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IServiceTypeRepository : IRepository<ServiceType>
{
    // Add custom methods for PolicyType here if needed
}

public class ServiceTypeRepository : Repository<ServiceType>, IServiceTypeRepository
{
    public ServiceTypeRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }

}