using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IHotelPolicyRepository : IRepository<HotelPolicy>
{
    // Add custom methods for HotelPolicy here if needed
}

public class HotelPolicyRepository : Repository<HotelPolicy>, IHotelPolicyRepository
{
    public HotelPolicyRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}