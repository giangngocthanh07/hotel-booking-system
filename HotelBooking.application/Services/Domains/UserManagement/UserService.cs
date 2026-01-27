using HotelBooking.application.Helpers;
using HotelBooking.application.Services;
using HotelBooking.infrastructure.Models;
using System.Net.Mail;

namespace HotelBooking.application.Services.Domains.UserManagement
{
    public interface IUserService
    {
        public Task<UserDetailDTO> GetByIdAsync(int id);
        public Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin);
        public Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer);
        public Task<ApiResponse<LoginResponseDTO>> LoginUser(LoginUserDTO userLogin);
        public Task<bool> ApproveUpgradeToOwnerAsync(int requestId, int adminId);
        // public Task<bool> RejectUpgradeToOwnerAsync(int requestId, int adminId);
    }

    public class UserService : IUserService
    {
        public HotelBookingDBContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUpgradeRequestRepository _upgradeRequestRepository;
        public JwtAuthService _jwtAuthService;
        public IUnitOfWork _dbu;

        public UserService(HotelBookingDBContext context, IUserRepository userRepository, IUserRoleRepository userRoleRepository, IUpgradeRequestRepository upgradeRequestRepository, JwtAuthService jwtAuthService, IUnitOfWork dbu)
        {
            _context = context;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _upgradeRequestRepository = upgradeRequestRepository;
            _jwtAuthService = jwtAuthService;
            _dbu = dbu;
        }

        public async Task<UserDetailDTO> GetByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null!;

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
            return userDetailDTO;
        }

        public async Task<ApiResponse<RegisterResponseDTO>> RegisterAdmin(RegisterAdminDTO newAdmin)
        {
            try
            {
                var checkAdmin = await _userRepository.SingleOrDefaultAsync(admin => admin.Email == newAdmin.Email || admin.UserName == newAdmin.Username);
                if (checkAdmin != null)
                {
                    return new ApiResponse<RegisterResponseDTO>
                    {
                        StatusCode = StatusCodeResponse.Conflict,
                        Content = new RegisterResponseDTO
                        {
                            IsSuccess = false,

                        },
                        Message = checkAdmin.UserName == newAdmin.Username ? MessageRegister.USERNAME_EXIST : MessageRegister.EMAIL_EXIST
                    };
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

                // Thêm newUser vào User
                await _userRepository.AddAsync(user);
                // Lưu thay đổi vào database
                await _dbu.SaveChangesAsync(); // Save to generate user.Id
                                               // Thêm Role vào bảng UserRoles
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = newAdmin.GetRoleId()
                };

                // Thêm bảng tham chiếu
                user.UserRoles.Add(userRole);
                await _dbu.SaveChangesAsync(); // Save again to save UserRole

                return new ApiResponse<RegisterResponseDTO>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Content = new RegisterResponseDTO
                    {
                        IsSuccess = true,

                        FullName = user.FullName,
                        Email = user.Email
                    },

                    Message = MessageRegister.REGISTER_SUCCESS
                };
            }
            catch (Exception)
            {
                return new ApiResponse<RegisterResponseDTO>
                {
                    StatusCode = StatusCodeResponse.Error,
                    Content = new RegisterResponseDTO
                    {
                        IsSuccess = false,

                    },
                    Message = MessageRegister.REGISTER_FAIL,
                };
            }
        }

        public async Task<ApiResponse<RegisterResponseDTO>> RegisterCustomer(RegisterCustomerDTO newCustomer)
        {
            try
            {
                // --- 1. VALIDATE INPUT (THÊM ĐOẠN NÀY) ---
                if (!IsValidEmail(newCustomer.Email))
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, MessageRegister.INVALID_EMAIL);
                }

                // --- 2. VALIDATE PASSWORD (THÊM ĐOẠN NÀY) ---
                // 1. Check Rỗng trước
                if (string.IsNullOrWhiteSpace(newCustomer.Password))
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, MessageRegister.EMPTY_PASSWORD);
                }

                // 2. Check độ dài
                if (newCustomer.Password.Length <= 8)
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, MessageRegister.SHORT_PASSWORD);
                }

                // 3. Check chữ hoa (Dùng LINQ cho gọn, khỏi cần Regex phức tạp)
                if (!newCustomer.Password.Any(char.IsUpper))
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, MessageRegister.UPPERCASE_LETTER_PASSWORD);
                }

                // 4. Check chữ thường
                if (!newCustomer.Password.Any(char.IsLower))
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, MessageRegister.LOWERCASE_LETTER_PASSWORD);
                }

                // 5. Check ký tự đặc biệt
                if (!newCustomer.Password.Any(ch => !char.IsLetterOrDigit(ch)))
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.BadRequest, MessageRegister.SPECIAL_CHARACTER_PASSWORD);
                }

                // --- 3. CHECK TRÙNG (Code cũ của bạn) ---
                // 1. Kiểm tra xem user đã tồn tại chưa
                var checkCustomer = await _userRepository.SingleOrDefaultAsync(customer =>
                    customer.Email == newCustomer.Email ||
                    customer.UserName == newCustomer.Username
                );

                // 2. Nếu tìm thấy (khác null) thì TRẢ VỀ LỖI NGAY LẬP TỨC
                if (checkCustomer != null)
                {
                    return ResponseFactory.Failure<RegisterResponseDTO>(StatusCodeResponse.Conflict, checkCustomer.UserName == newCustomer.Username ? MessageRegister.USERNAME_EXIST : MessageRegister.EMAIL_EXIST);
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

                // Thêm newUser vào User
                await _userRepository.AddAsync(user);
                await _dbu.SaveChangesAsync(); // Save to generate user.Id

                // Thêm Role vào bảng UserRoles
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = newCustomer.GetRoleId()
                };

                // Thêm bảng tham chiếu
                user.UserRoles.Add(userRole);
                // Lưu thay đổi vào database
                await _dbu.SaveChangesAsync();

                return ResponseFactory.Success<RegisterResponseDTO>(new RegisterResponseDTO
                {
                    IsSuccess = true,

                    FullName = user.FullName,
                    Email = user.Email
                }, MessageRegister.REGISTER_SUCCESS);
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
                var user = await _userRepository.GetUserWithRoles(u => u.UserName == userLogin.UsernameOrEmail || u.Email == userLogin.UsernameOrEmail);
                if (user == null)
                {
                    return new ApiResponse<LoginResponseDTO> { StatusCode = StatusCodeResponse.NotFound, Message = MessageLogin.USER_NOT_FOUND, Content = null };
                }

                // Kiểm tra mật khẩu
                if (!PasswordHelper.VerifyPassword(userLogin.Password, user.PasswordHash))
                {
                    return new ApiResponse<LoginResponseDTO> { StatusCode = StatusCodeResponse.NotFound, Message = MessageLogin.PASSWORD_INCORRECT, Content = null };
                }
                var token = _jwtAuthService.GenerateToken(user);
                var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                return new ApiResponse<LoginResponseDTO>
                {
                    StatusCode = StatusCodeResponse.Success,
                    Message = MessageLogin.LOGIN_SUCCESS,
                    Content = new LoginResponseDTO
                    {
                        AccessToken = token,
                        FullName = user.FullName!,
                        AvatarUrl = user.AvatarUrl,
                        Roles = roles,
                    }
                };
            }
            catch (Exception)
            {
                return new ApiResponse<LoginResponseDTO> { StatusCode = StatusCodeResponse.Error, Message = MessageLogin.ERROR_IN_SERVER, Content = null };
            }
        }

        public async Task<bool> ApproveUpgradeToOwnerAsync(int requestId, int adminId)
        {
            var request = await _upgradeRequestRepository
            .SingleOrDefaultAsync(r => r.Id == requestId && r.Status == "Pending");
            if (request == null)
                return false;

            var user = await _userRepository.SingleOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
                return false;

            // Kiểm tra xem user đã có role Owner chưa
            var hasOwnerRole = await _userRoleRepository
                .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == RoleTypeConstDTO.Owner);

            if (!hasOwnerRole)
            {
                // Thêm role Owner cho user
                var newUserRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = RoleTypeConstDTO.Owner
                };
                _context.UserRoles.Add(newUserRole);

                // Cập nhật trạng thái yêu cầu
                request.Status = "Approved";
                request.ApprovedAt = DateTime.Now;
                request.ApprovedBy = adminId;

                await _upgradeRequestRepository.UpdateAsync(request);


                await _dbu.SaveChangesAsync();
                return true;
            }

            return false;
        }

        // Hàm Helper check Regex (có thể để private ở dưới cùng class Service)
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                // Regex đơn giản để check email
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