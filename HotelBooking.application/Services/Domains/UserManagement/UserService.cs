using HotelBooking.application.Helpers;
using FluentValidation;
using HotelBooking.infrastructure.Models;
using System.Net.Mail;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.Services.Domains.Auth;
// Note: MessageRegister and MessageLogin are consolidated into MessageResponse in Helpers/Messages/
// Use MessageResponse.UserManagement.Register.* and MessageResponse.UserManagement.Login.* for new code
// Or keep MessageRegister/MessageLogin for backward compatibility

namespace HotelBooking.application.Services.Domains.UserManagement
{
    public interface IUserService
    {
        public Task<ApiResponse<UserDetailDTO>> GetByIdAsync(int id);
        public Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin);
        public Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer);
        public Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginUserDTO userLogin);
        public Task<ApiResponse<bool>> ApproveUpgradeToOwnerAsync(int requestId, int adminId);
        // public Task<bool> RejectUpgradeToOwnerAsync(int requestId, int adminId);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUpgradeRequestRepository _upgradeRequestRepository;
        public JwtAuthService _jwtAuthService;
        public IUnitOfWork _dbu;

        private readonly IValidator<RegisterCustomerDTO> _registerCustomerValidator;
        private readonly IValidator<RegisterAdminDTO> _registerAdminValidator;
        private readonly IValidator<LoginUserDTO> _loginValidator;

        public UserService(IUserRepository userRepository,
                           IUserRoleRepository userRoleRepository,
                           IUpgradeRequestRepository upgradeRequestRepository,
                           JwtAuthService jwtAuthService,
                           IUnitOfWork dbu,
                           IValidator<RegisterCustomerDTO> registerCustomerValidator,
                           IValidator<RegisterAdminDTO> registerAdminValidator,
                           IValidator<LoginUserDTO> loginValidator)
        {
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _upgradeRequestRepository = upgradeRequestRepository;
            _jwtAuthService = jwtAuthService;
            _dbu = dbu;

            _registerCustomerValidator = registerCustomerValidator;
            _registerAdminValidator = registerAdminValidator;
            _loginValidator = loginValidator;
        }

        public async Task<ApiResponse<UserDetailDTO>> GetByIdAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.NotFound, MessageResponse.UserManagement.User.NOT_FOUND);
                }

                var userWithRoles = await _userRepository.GetUserWithRoles(u => u.UserName == user.UserName || u.Email == user.Email);

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
            catch (Exception)
            {
                return ResponseFactory.Failure<UserDetailDTO>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
            }
        }

        public async Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin)
        {
            try
            {
                // Validate input using injected FluentValidation validator
                var adminValidation = _registerAdminValidator.Validate(newAdmin);
                if (!adminValidation.IsValid)
                {
                    var response = ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, adminValidation.Errors.First().ErrorMessage);
                    response.Content = new RegisterResponseDTO { IsSuccess = false };
                    return response;
                }

                var checkAdmin = await _userRepository.SingleOrDefaultAsync(admin => admin.Email == newAdmin.Email || admin.UserName == newAdmin.Username);
                if (checkAdmin != null)
                {
                    var response = ResponseFactory.Failure<RegisterResponseDTO>(
                        StatusCodeResponse.Conflict,
                        checkAdmin.UserName == newAdmin.Username
                            ? MessageResponse.UserManagement.Register.USERNAME_EXIST
                            : MessageResponse.UserManagement.Register.EMAIL_EXIST);
                    response.Content = new RegisterResponseDTO { IsSuccess = false };
                    return response;
                }

                var user = new User
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

                // Add new user to Users table
                await _userRepository.AddAsync(user);
                // Persist to database
                await _dbu.SaveChangesAsync(); // Save to generate user.Id
                                               // Add role to UserRoles table
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = newAdmin.GetRoleId()
                };

                // Add role reference
                user.UserRoles.Add(userRole);
                await _dbu.SaveChangesAsync(); // Save again to save UserRole

                return ResponseFactory.Success(new RegisterResponseDTO
                {
                    IsSuccess = true,
                    FullName = user.FullName,
                    Email = user.Email
                }, MessageResponse.UserManagement.Register.SUCCESS);
            }
            catch (Exception)
            {
                var response = ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.Error, MessageResponse.UserManagement.Register.FAIL);
                response.Content = new RegisterResponseDTO { IsSuccess = false };
                return response;
            }
        }

        [Obsolete]
        public async Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer)
        {
            try
            {
                // Validate input using injected FluentValidation validator
                var validation = _registerCustomerValidator.Validate(newCustomer);
                if (!validation.IsValid)
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, validation.Errors.First().ErrorMessage);
                }

                // --- 3. CHECK DUPLICATES ---
                // 1. Check if user already exists
                var checkCustomer = await _userRepository.SingleOrDefaultAsync(customer =>
                    customer.Email == newCustomer.Email ||
                    customer.UserName == newCustomer.Username
                );

                // 2. If found (non-null) -> return error immediately
                if (checkCustomer != null)
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.Conflict, checkCustomer.UserName == newCustomer.Username ? MessageResponse.UserManagement.Register.USERNAME_EXIST : MessageResponse.UserManagement.Register.EMAIL_EXIST);
                }

                var user = new User
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

                // Add new user to Users table
                await _userRepository.AddAsync(user);
                await _dbu.SaveChangesAsync(); // Save to generate user.Id

                // Add role to UserRoles table
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = newCustomer.GetRoleId()
                };

                // Add role reference
                user.UserRoles.Add(userRole);
                // Persist changes to database
                await _dbu.SaveChangesAsync();

                return ResponseFactory.Success(new RegisterResponseDTO
                {
                    IsSuccess = true,

                    FullName = user.FullName,
                    Email = user.Email
                }, MessageResponse.UserManagement.Register.SUCCESS);
            }
            catch (Exception)
            {
                return ResponseFactory.ServerError<RegisterResponseDTO>();
            }
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
                if (user == null || !PasswordHelper.VerifyPassword(userLogin.Password, user.PasswordHash))
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
            catch (Exception)
            {
                return ResponseFactory.Failure<LoginResponseDTO>(StatusCodeResponse.Error, MessageResponse.UserManagement.Login.ERROR_IN_SERVER);
            }
        }

        public async Task<ApiResponse<bool>> ApproveUpgradeToOwnerAsync(int requestId, int adminId)
        {
            try
            {
                var request = await _upgradeRequestRepository
                    .SingleOrDefaultAsync(r => r.Id == requestId && r.Status == "Pending");
                if (request == null)
                {
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.BadRequest, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_STATUS_INVALID);
                }

                var user = await _userRepository.SingleOrDefaultAsync(u => u.Id == request.UserId);
                if (user == null)
                {
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.NotFound, MessageResponse.RequestManagement.UpgradeRequest.USER_NOT_FOUND);
                }

                // Check if user already has Owner role
                var hasOwnerRole = await _userRoleRepository
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == RoleTypeConstDTO.Owner);

                if (hasOwnerRole)
                {
                    return ResponseFactory.Failure<bool>(StatusCodeResponse.Conflict, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVE_FAILED);
                }

                // Assign Owner role to user
                var newUserRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = RoleTypeConstDTO.Owner
                };
                await _userRoleRepository.AddAsync(newUserRole);

                // Update request status
                request.Status = "Approved";
                request.ApprovedAt = DateTime.Now;
                request.ApprovedBy = adminId;

                await _upgradeRequestRepository.UpdateAsync(request);

                var saved = await _dbu.SaveChangesAsync() > 0;
                return saved
                    ? ResponseFactory.Success(true, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVED_SUCCESS)
                    : ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.RequestManagement.UpgradeRequest.REQUEST_APPROVE_FAILED);
            }
            catch (Exception)
            {
                return ResponseFactory.Failure<bool>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
            }
        }

        // Helper method to validate email format (can be private at the bottom of the Service class)
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                // Simple regex to validate email format
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}