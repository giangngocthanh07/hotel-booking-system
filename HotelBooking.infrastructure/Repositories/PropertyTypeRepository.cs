using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IPropertyTypeRepository : IRepository<PropertyType>
{
    // Add custom methods for PropertyType here if needed
}

public class PropertyTypeRepository : Repository<PropertyType>, IPropertyTypeRepository
{
    public PropertyTypeRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider)
    {
    }
}