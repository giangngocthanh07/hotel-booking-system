using HotelBooking.infrastructure.Models;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.Helpers;

namespace HotelBooking.application.Services.Domains.AdminManagement
{
    public interface IRoleService
    {
        public Task<ApiResponse<bool>> AddAsync(RoleDTO entity);
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

        public async Task<ApiResponse<bool>> AddAsync(RoleDTO newRole)
        {
            try
            {
                var checkRole = await _roleRepository.SingleOrDefaultAsync(r => r.Name == newRole.Name);
                if (checkRole != null)
                {
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.Conflict, MessageResponse.AdminManagement.Role.NAME_ALREADY_EXISTS);
                }

                Role role = new Role();
                role.Name = newRole.Name;
                role.Description = newRole.Description;
                role.IsDeleted = false;
                await _roleRepository.AddAsync(role);
                await _dbu.SaveChangesAsync();
                return ResponseFactory.Success<bool>(true, MessageResponse.AdminManagement.Role.ADD_SUCCESS);

            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<bool>();
            }
        }
    }
}