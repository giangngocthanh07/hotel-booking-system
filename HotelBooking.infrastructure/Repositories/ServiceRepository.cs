using HotelBooking.infrastructure.Models;

public interface IServiceRepository : IRepository<Service> { }
public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(HotelBookingContext context) : base(context) { }
}
