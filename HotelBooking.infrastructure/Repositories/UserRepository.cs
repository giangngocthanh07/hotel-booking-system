using HotelBooking.infrastructure.Models;

public interface IUserRepository : IRepository<User> { }
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HotelBookingContext context) : base(context) { }
}
