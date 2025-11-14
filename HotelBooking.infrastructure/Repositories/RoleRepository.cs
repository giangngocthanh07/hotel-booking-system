using HotelBooking.infrastructure.Models;

public interface IRoleRepository : IRepository<Role> { }
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(HotelBookingDBContext context) : base(context) { }
}
