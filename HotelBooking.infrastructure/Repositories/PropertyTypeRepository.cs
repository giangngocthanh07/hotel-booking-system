using HotelBooking.infrastructure.Models;

public interface IPropertyTypeRepository : IRepository<PropertyType>
{
    // Add custom methods for PropertyType here if needed
}

public class PropertyTypeRepository : Repository<PropertyType>, IPropertyTypeRepository
{
    public PropertyTypeRepository(HotelBookingDBContext context) : base(context)
    {
    }
}