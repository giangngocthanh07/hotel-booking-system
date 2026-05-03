using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.EntityFrameworkCore;

public interface IUserRoleRepository : IRepository<UserRole> { }
public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }

    
}
