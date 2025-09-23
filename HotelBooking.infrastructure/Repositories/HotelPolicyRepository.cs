using HotelBooking.infrastructure.Models;

public interface IHotelPolicyRepository : IRepository<HotelPolicy>
{
    // Add custom methods for HotelPolicy here if needed
}

public class HotelPolicyRepository : Repository<HotelPolicy>, IHotelPolicyRepository
{
    public HotelPolicyRepository(HotelBookingContext context) : base(context)
    {
    }
}