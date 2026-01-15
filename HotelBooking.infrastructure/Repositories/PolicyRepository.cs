using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IPolicyRepository : IRepository<Policy>
{
}
public class PolicyRepository : Repository<Policy>, IPolicyRepository
{
    public PolicyRepository(HotelBookingDBContext context) : base(context) { }
}
