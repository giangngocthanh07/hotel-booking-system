using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;

public interface IRoleRepository : IRepository<Role> { }
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(HotelBookingDBContext context, ICancellationTokenProvider tokenProvider) : base(context, tokenProvider) { }
}
