using FluentValidation;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Helpers;
using HotelBooking.application.Helpers.Infrastructure;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.UserManagement.Register
{
    public interface IRegisterService
    {
        Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer);
        Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin);
    }
    public class RegisterService : IRegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IValidator<RegisterCustomerDTO> _registerCustomerValidator;
        private readonly IValidator<RegisterAdminDTO> _registerAdminValidator;
        private readonly IUnitOfWork _dbu;
        private readonly ILogger<RegisterService> _logger;


        public RegisterService(IUserRepository userRepository, IUserRoleRepository userRoleRepository, IValidator<RegisterCustomerDTO> registerCustomerValidator, IValidator<RegisterAdminDTO> registerAdminValidator, IUnitOfWork dbu, ILogger<RegisterService> logger)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _registerCustomerValidator = registerCustomerValidator;
            _registerAdminValidator = registerAdminValidator;
            _dbu = dbu;
            _logger = logger;
        }

        public async Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer)
        {
            var validationResult = await _registerCustomerValidator.ValidateAsync(newCustomer);
            if (!validationResult.IsValid)
            {
                return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, validationResult.Errors.First().ErrorMessage);
            }

            return await ExecuteRegistrationAsync(
                newCustomer.Username,
                newCustomer.FullName,
                newCustomer.Email,
                newCustomer.PhoneNumber,
                newCustomer.Password,
                newCustomer.GetRoleId(),
                "RegisterCustomer"
            );
        }

        public async Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin)
        {
            var validationResult = await _registerAdminValidator.ValidateAsync(newAdmin);
            if (!validationResult.IsValid)
            {
                return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, validationResult.Errors.First().ErrorMessage);
            }

            return await ExecuteRegistrationAsync(
                newAdmin.Username,
                newAdmin.FullName,
                newAdmin.Email,
                newAdmin.PhoneNumber,
                newAdmin.Password,
                newAdmin.GetRoleId(),
                "RegisterAdmin"
            );
        }

        private async Task<ApiResponse<RegisterResponseDTO>> ExecuteRegistrationAsync(
            string username, string fullName, string email, string phoneNumber, string password, int roleId, string methodName)
        {
            try
            {
                var existingUser = await _userRepository.SingleOrDefaultAsync(u => u.Email == email || u.UserName == username);
                if (existingUser != null)
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(
                        StatusCodeResponse.Conflict, 
                        existingUser.UserName == username ? MessageResponse.UserManagement.Register.USERNAME_EXIST : MessageResponse.UserManagement.Register.EMAIL_EXIST);
                }

                await _dbu.BeginTransactionAsync();

                User newUser = new User
                {
                    UserName = username,
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    PasswordHash = PasswordHelper.HashPassword(password),
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    DateOfBirth = null,
                    IsActive = true
                };

                await _userRepository.AddAsync(newUser);
                await _dbu.SaveChangesAsync();

                UserRole newUserRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = roleId
                };

                await _userRoleRepository.AddAsync(newUserRole);
                var saved = await _dbu.SaveChangesAsync() > 0;

                if (saved)
                {
                    await _dbu.CommitTransactionAsync();
                    return ResponseFactory.Success(new RegisterResponseDTO
                    {
                        Username = newUser.UserName,
                        FullName = newUser.FullName,
                        Email = newUser.Email
                    }, MessageResponse.UserManagement.Register.SUCCESS);
                }
                else
                {
                    await _dbu.RollBackTransactionAsync();
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("RegisterService.{MethodName}: {ErrorMessage}", methodName, ex.Message);
                await _dbu.RollBackTransactionAsync();
                return ResponseFactory.ServerError<RegisterResponseDTO>();
            }
        }
    }
}