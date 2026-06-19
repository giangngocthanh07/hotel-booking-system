using HotelBooking.application.Helpers;
using HotelBooking.application.DTOs.User;
using HotelBooking.application.Services.Domains.UserManagement.Register;
using HotelBooking.application.Services.Domains.UserManagement.Login;
using FluentValidation;

// Note: MessageRegister and MessageLogin are consolidated into MessageResponse in Helpers/Messages/
// Use MessageResponse.UserManagement.Register.* and MessageResponse.UserManagement.Login.* for new code
// Or keep MessageRegister/MessageLogin for backward compatibility

namespace HotelBooking.application.Services.Domains.UserManagement
{
    public interface IUserService
    {
        IRegisterService Register { get; }
        ILoginService Login { get; }

        Task<ApiResponse<UserDetailDTO>> GetByIdAsync(int id);
        Task<ApiResponse<UserDetailDTO>> UpdateProfileAsync(int userId, UpdateUserProfileDTO request);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IValidator<UpdateUserProfileDTO> _updateProfileValidator;
        private readonly IUnitOfWork _dbu;
        public IRegisterService Register { get; }
        public ILoginService Login { get; }
        private readonly ILogger<UserService> _logger;


        public UserService(IRegisterService register, ILoginService login, IUserRepository userRepository, ILogger<UserService> logger, IValidator<UpdateUserProfileDTO> updateProfileValidator, IUnitOfWork dbu)
        {
            Register = register;
            Login = login;
            _userRepository = userRepository;
            _logger = logger;
            _updateProfileValidator = updateProfileValidator;
            _dbu = dbu;
        }

        public async Task<ApiResponse<UserDetailDTO>> UpdateProfileAsync(int userId, UpdateUserProfileDTO request)
        {
            try
            {
                var validationResult = await _updateProfileValidator.ValidateAsync(request);
                if (!validationResult.IsValid)
                {
                    return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.BadRequest, validationResult.Errors.First().ErrorMessage);
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.NotFound, MessageResponse.UserManagement.User.NOT_FOUND);
                }

                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;
                user.DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.Value) : null;

                await _userRepository.UpdateAsync(user);
                var saved = await _dbu.SaveChangesAsync() > 0;

                if (saved)
                {
                    return await GetByIdAsync(userId); // Return the updated profile
                }

                return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.Error, MessageResponse.Common.UPDATE_FAILED);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                return ResponseFactory.ServerError<UserDetailDTO>();
            }
        }

        public async Task<ApiResponse<UserDetailDTO>> GetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.BadRequest, MessageResponse.Common.INVALID_ID);
                }

                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.NotFound, MessageResponse.UserManagement.User.NOT_FOUND);
                }

                var userWithRoles = await _userRepository.GetUserWithRoles(u => u.UserName == user.UserName || u.Email == user.Email);

                if (userWithRoles == null)
                {
                    return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.NotFound, MessageResponse.UserManagement.User.NOT_FOUND);
                }

                var userDetailDTO = new UserDetailDTO
                {
                    Id = userWithRoles.Id,
                    UserName = userWithRoles.UserName,
                    FullName = userWithRoles.FullName!,
                    Email = userWithRoles.Email,
                    PhoneNumber = userWithRoles.PhoneNumber,
                    DateOfBirth = userWithRoles.DateOfBirth,
                    AvatarUrl = userWithRoles.AvatarUrl,
                    IsActive = userWithRoles.IsActive,
                    IsDeleted = userWithRoles.IsDeleted,
                    CreatedAt = userWithRoles.CreatedAt,
                    Roles = userWithRoles.UserRoles.Select(ur => ur.Role.Name).ToList()
                };
                return ResponseFactory.Success(userDetailDTO, MessageResponse.Common.GET_SUCCESSFULLY);
            }
            catch (Exception ex)
            {
                _logger.LogError("UserService.GetByIdAsync: {ErrorMessage}", ex.Message);
                return ResponseFactory.ServerError<UserDetailDTO>();
            }
        }

        // public async Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin)
        // {
        //     try
        //     {
        //         // Validate input using injected FluentValidation validator
        //         var adminValidation = _registerAdminValidator.Validate(newAdmin);
        //         if (!adminValidation.IsValid)
        //         {
        //             var response = ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, adminValidation.Errors.First().ErrorMessage);
        //             response.Content = null;
        //             return response;
        //         }

        //         var checkAdmin = await _userRepository.SingleOrDefaultAsync(admin => admin.Email == newAdmin.Email || admin.UserName == newAdmin.Username);
        //         if (checkAdmin != null)
        //         {
        //             var response = ResponseFactory.Failure<RegisterResponseDTO>(
        //                 StatusCodeResponse.Conflict,
        //                 checkAdmin.UserName == newAdmin.Username
        //                     ? MessageResponse.UserManagement.Register.USERNAME_EXIST
        //                     : MessageResponse.UserManagement.Register.EMAIL_EXIST);
        //             response.Content = null;
        //             return response;
        //         }

        //         var user = new User
        //         {
        //             UserName = newAdmin.Username,
        //             FullName = newAdmin.FullName,
        //             Email = newAdmin.Email,
        //             PhoneNumber = newAdmin.PhoneNumber,
        //             PasswordHash = PasswordHelper.HashPassword(newAdmin.Password),
        //             IsDeleted = false,
        //             CreatedAt = DateTime.Now,
        //             DateOfBirth = null,
        //             IsActive = true
        //         };

        //         // Start transaction
        //         await _dbu.BeginTransactionAsync();

        //         // Add new user to Users table
        //         await _userRepository.AddAsync(user);
        //         // Persist to database
        //         await _dbu.SaveChangesAsync(); // Save to generate user.Id
        //                                        // Add role to UserRoles table
        //         var userRole = new UserRole
        //         {
        //             UserId = user.Id,
        //             RoleId = newAdmin.GetRoleId()
        //         };


        //         // Add role reference
        //         user.UserRoles.Add(userRole);
        //         await _dbu.SaveChangesAsync(); // Save again to save UserRole

        //         await _dbu.CommitTransactionAsync(); // Commit transaction

        //         return ResponseFactory.Success(new RegisterResponseDTO
        //         {
        //             FullName = user.FullName,
        //             Email = user.Email
        //         }, MessageResponse.UserManagement.Register.SUCCESS);
        //     }
        //     catch (Exception)
        //     {
        //         await _dbu.RollBackTransactionAsync(); // Rollback transaction on error
        //         return ResponseFactory.ServerError<RegisterResponseDTO>();
        //     }
        // }


    }
}