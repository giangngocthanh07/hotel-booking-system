using FluentValidation;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.Helpers;
using HotelBooking.application.Helpers.Infrastructure;
using HotelBooking.application.Services.Domains.Auth;
using HotelBooking.application.Validators.UserManagement.Login;

namespace HotelBooking.application.Services.Domains.UserManagement.Login
{
    public interface ILoginService
    {
        Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginUserDTO userLogin);
    }

    public class LoginService : ILoginService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtAuthService _jwtAuthService;
        private readonly IValidator<LoginUserDTO> _loginValidator;
        private readonly ILogger<LoginService> _logger;


        public LoginService(IUserRepository userRepository, IJwtAuthService jwtAuthService, IValidator<LoginUserDTO> loginValidator, ILogger<LoginService> logger)
        {
            _userRepository = userRepository;
            _jwtAuthService = jwtAuthService;
            _loginValidator = loginValidator;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginUserDTO userLogin)
        {
            try
            {
                // Validate login input using injected validator
                var loginValidation = _loginValidator.Validate(userLogin);
                if (!loginValidation.IsValid)
                {
                    return ResponseFactory.Failure<LoginResponseDTO>(StatusCodeResponse.BadRequest, loginValidation.Errors.First().ErrorMessage);
                }

                var user = await _userRepository.GetUserWithRoles(u => u.UserName == userLogin.UsernameOrEmail || u.Email == userLogin.UsernameOrEmail);

                // If user not found or password does not match -> return Unauthorized
                if (user == null || !PasswordHelper.VerifyPassword(userLogin.Password, user.PasswordHash) || user.IsActive == false || user.IsDeleted == true)
                {
                    return ResponseFactory.Failure<LoginResponseDTO>(
                        StatusCodeResponse.Unauthorized,
                        MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
                }

                // Generate JWT token
                var token = _jwtAuthService.GenerateToken(user);
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                return ResponseFactory.Success(new LoginResponseDTO
                {
                    AccessToken = token,
                    FullName = user.FullName!,
                    AvatarUrl = user.AvatarUrl,
                    Roles = roles,
                }, MessageResponse.UserManagement.Login.SUCCESS);
            }
            catch (Exception ex)
            {
                _logger.LogError("LoginService.LoginUser: {ErrorMessage}", ex.Message);
                return ResponseFactory.Failure<LoginResponseDTO>(StatusCodeResponse.Error, MessageResponse.UserManagement.Login.ERROR_IN_SERVER);
            }
        }
    }
}