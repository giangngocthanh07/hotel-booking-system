using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IServiceRepository : IRepository<Service>
{
}
public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}

