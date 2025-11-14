using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserRepository : IRepository<User> {
    Task<User> GetUserWithRoles(Expression<Func<User, bool>> predicate);
}
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(HotelBookingDBContext context) : base(context) { }

    public async Task<User> GetUserWithRoles(Expression<Func<User,bool>> predicate)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(predicate);
    }
}
