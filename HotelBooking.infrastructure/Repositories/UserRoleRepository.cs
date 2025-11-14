using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserRoleRepository : IRepository<UserRole> { }
public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(HotelBookingDBContext context) : base(context) { }

    
}
