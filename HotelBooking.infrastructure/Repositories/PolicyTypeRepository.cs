using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IPolicyTypeRepository : IRepository<PolicyType>
{
    // Add custom methods for PolicyType here if needed
}

public class PolicyTypeRepository : Repository<PolicyType>, IPolicyTypeRepository
{
    public PolicyTypeRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}