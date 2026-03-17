using HotelBooking.application.DTOs.User.Login;

namespace HotelBooking.application.Services.Domains.UserManagement.Login
{
    public interface ILoginService
    {
        Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginUserDTO userLogin);
    }

    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _dbu;

        public LoginService(IUserRepository userRepository, IUnitOfWork dbu)
        {
            _userRepository = userRepository;
            _dbu = dbu;
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginUserDTO userLogin)
        {
            throw new NotImplementedException();
        }
    }
}