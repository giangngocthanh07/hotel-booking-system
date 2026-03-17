using FluentValidation;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Helpers;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.application.Services.Domains.UserManagement.Register
{
    public interface IRegisterService
    {
        Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer);
    }
    public class RegisterService : IRegisterService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IValidator<RegisterCustomerDTO> _registerCustomerValidator;
        private readonly IUnitOfWork _dbu;

        public RegisterService(IUserRepository userRepository, IUserRoleRepository userRoleRepository, IValidator<RegisterCustomerDTO> registerCustomerValidator, IUnitOfWork dbu)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _registerCustomerValidator = registerCustomerValidator;
            _dbu = dbu;
        }

        public async Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer)
        {
            // 1. Validate the incoming DTO
            var validationResult = await _registerCustomerValidator.ValidateAsync(newCustomer);
            if (!validationResult.IsValid)
            {
                return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, validationResult.Errors.First().ErrorMessage);
            }

            // 2. Check if the user already exists (by email or username)
            var existingUser = await _userRepository.SingleOrDefaultAsync(u => u.Email == newCustomer.Email || u.UserName == newCustomer.Username);
            if (existingUser != null)
            {
                return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.Conflict, existingUser.UserName == newCustomer.Username ? MessageResponse.UserManagement.Register.USERNAME_EXIST : MessageResponse.UserManagement.Register.EMAIL_EXIST);
            }

            try
            {
                await _dbu.BeginTransactionAsync();

                User newUser = new User
                {
                    UserName = newCustomer.Username,
                    FullName = newCustomer.FullName,
                    Email = newCustomer.Email,
                    PhoneNumber = newCustomer.PhoneNumber,
                    PasswordHash = PasswordHelper.HashPassword(newCustomer.Password),
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    DateOfBirth = null,
                    IsActive = true
                };

                await _userRepository.AddAsync(newUser);
                await _dbu.SaveChangesAsync();

                // Assign the default "Customer" role to the new user
                UserRole newUserRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = newCustomer.GetRoleId() // This will return the constant RoleId for Customer
                };

                await _userRoleRepository.AddAsync(newUserRole);
                await _dbu.SaveChangesAsync();

                await _dbu.CommitTransactionAsync();

                return ResponseFactory.Success(new RegisterResponseDTO
                {
                    Username = newUser.UserName,
                    FullName = newUser.FullName,
                    Email = newUser.Email
                }, MessageResponse.UserManagement.Register.SUCCESS);

            }
            catch (Exception)
            {
                await _dbu.RollBackTransactionAsync();
                return ResponseFactory.ServerError<RegisterResponseDTO>();
            }

        }
    }
}