using HotelBooking.infrastructure.Models;

public interface IServiceRepository : IRepository<Service> { }
public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(HotelBookingDBContext context) : base(context) { }
}
