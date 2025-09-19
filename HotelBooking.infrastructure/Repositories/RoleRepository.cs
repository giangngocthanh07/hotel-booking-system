using HotelBooking.infrastructure.Models;

public interface IRoleRepository : IRepository<Role> { }
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(HotelBookingContext context) : base(context) { }
}
