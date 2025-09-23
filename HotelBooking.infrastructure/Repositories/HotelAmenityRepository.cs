using HotelBooking.infrastructure.Models;

public interface IHotelAmenityRepository : IRepository<HotelAmenity>
{
    // Add custom methods for HotelAmenity here if needed
}

public class HotelAmenityRepository : Repository<HotelAmenity>, IHotelAmenityRepository
{
    public HotelAmenityRepository(HotelBookingContext context) : base(context)
    {
    }
}