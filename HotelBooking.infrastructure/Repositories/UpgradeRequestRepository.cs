using HotelBooking.infrastructure.Models;

public interface IUpgradeRequestRepository : IRepository<UpgradeRequest>
{
    // Add custom methods for UpgradeRequest here if needed
}

public class UpgradeRequestRepository : Repository<UpgradeRequest>, IUpgradeRequestRepository
{
    public UpgradeRequestRepository(HotelBookingContext context) : base(context)
    {
    }
}