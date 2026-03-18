using FluentValidation;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Helpers;
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

        public RegisterService(IUserRepository userRepository, IUserRoleRepository userRoleRepository, IValidator<RegisterCustomerDTO> registerCustomerValidator, IValidator<RegisterAdminDTO> registerAdminValidator, IUnitOfWork dbu)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _registerCustomerValidator = registerCustomerValidator;
            _registerAdminValidator = registerAdminValidator;
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

        public async Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin)
        {
            // 1. Validate the incoming DTO
            var validationResult = await _registerAdminValidator.ValidateAsync(newAdmin);
            if (validationResult.IsValid == false)
            {
                return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, validationResult.Errors.First().ErrorMessage);
            }

            // 2. Check if the user already exists (by email or username)
            var existingUser = await _userRepository.SingleOrDefaultAsync(u => u.Email == newAdmin.Email || u.UserName == newAdmin.Username);
            if (existingUser != null)
            {
                return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.Conflict, existingUser.UserName == newAdmin.Username ? MessageResponse.UserManagement.Register.USERNAME_EXIST : MessageResponse.UserManagement.Register.EMAIL_EXIST);
            }

            // 3. If validation passes and user doesn't exist, create new User and UserRole entries in a transaction
            try
            {
                await _dbu.BeginTransactionAsync();

                User newUser = new User
                {
                    UserName = newAdmin.Username,
                    FullName = newAdmin.FullName,
                    Email = newAdmin.Email,
                    PhoneNumber = newAdmin.PhoneNumber,
                    PasswordHash = PasswordHelper.HashPassword(newAdmin.Password),
                    IsDeleted = false,
                    CreatedAt = DateTime.Now,
                    DateOfBirth = null,
                    IsActive = true
                };

                await _userRepository.AddAsync(newUser);
                await _dbu.SaveChangesAsync();

                // Assign the "Admin" role to the new user
                UserRole newUserRole = new UserRole
                {
                    UserId = newUser.Id,
                    RoleId = newAdmin.GetRoleId() // This will return the constant RoleId for Admin

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