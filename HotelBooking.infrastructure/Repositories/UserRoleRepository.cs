using HotelBooking.infrastructure.Models;

public interface IUserRoleRepository : IRepository<UserRole> { }
public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(HotelBookingContext context) : base(context) { }
}
