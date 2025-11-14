using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;

public interface IRoleService
{
    public Task<bool> AddAsync(RoleDTO entity);
}

public class RoleService : IRoleService
{
    HotelBookingDBContext _context;
    private readonly IRoleRepository _roleRepository;
    IUnitOfWork _dbu;

    public RoleService(HotelBookingDBContext context, IRoleRepository roleRepository, IUnitOfWork dbu)
    {
        _context = context;
        _roleRepository = roleRepository;
        _dbu = dbu;
    }

    public async Task<bool> AddAsync(RoleDTO newRole)
    {
        try
        {
            var checkRole = await _roleRepository.SingleOrDefaultAsync(r => r.Name == newRole.Name);
            if (checkRole != null)
            {
                return false;
            }

            Role role = new Role();
            role.Name = newRole.Name;
            role.Description = newRole.Description;
            role.IsDeleted = false;
            await _roleRepository.AddAsync(role);
            await _dbu.SaveChangesAsync();
            return true;

        }
        catch (Exception error)
        {
            throw new Exception(error.Message);
        }
    }
}