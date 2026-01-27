using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    public interface IRoleService
    {
        public Task<bool> AddAsync(RoleDTO entity);
    }

    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        IUnitOfWork _dbu;

        public RoleService(IRoleRepository roleRepository, IUnitOfWork dbu)
        {
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
}