using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;

public interface IPolicyRepository : IRepository<Policy>
{
}
public class PolicyRepository : Repository<Policy>, IPolicyRepository
{
    public PolicyRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
