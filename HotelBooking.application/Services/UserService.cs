using System.Linq.Expressions;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserService
{
    public Task<bool> RegisterAdmin(RegisterAdminDTO newAdmin);
    public Task<bool> RegisterCustomer(RegisterCustomerDTO newCustomer);
    public Task<bool> RequestUpgradeToOwnerAsync(int userId);
    public Task<string> LoginUser(LoginUserDTO userLogin);

    public Task<bool> ApproveUpgradeToOwnerAsync(int requestId, int adminId);
    // public Task<bool> RejectUpgradeToOwnerAsync(int requestId, int adminId);
}

public class UserService : IUserService
{
    public HotelBookingContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUpgradeRequestRepository _upgradeRequestRepository;
    public JwtAuthService _jwtAuthService;
    public IUnitOfWork _dbu;

    public UserService(HotelBookingContext context, IUserRepository userRepository, IUserRoleRepository userRoleRepository, IUpgradeRequestRepository upgradeRequestRepository, JwtAuthService jwtAuthService, IUnitOfWork dbu)
    {
        _context = context;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _upgradeRequestRepository = upgradeRequestRepository;
        _jwtAuthService = jwtAuthService;
        _dbu = dbu;
    }

    public async Task<bool> RegisterAdmin(RegisterAdminDTO newAdmin)
    {
        try
        {
            var checkAdmin = await _userRepository.SingleOrDefaultAsync(admin => admin.Email == newAdmin.Email || admin.UserName == newAdmin.Username);
            if (checkAdmin != null)
            {
                return false;
            }

            User user = new User();
            user.UserName = newAdmin.Username;
            user.FullName = newAdmin.FullName;
            user.Email = newAdmin.Email;
            user.PhoneNumber = newAdmin.PhoneNumber;
            user.PasswordHash = PasswordHelper.HashPassword(newAdmin.Password);
            user.IsDeleted = false;
            user.CreatedAt = DateTime.Now;
            user.DateOfBirth = null;
            user.IsActive = true;

            // Thêm Role vào bảng UserRoles
            UserRole userRole = new UserRole();
            userRole.UserId = user.Id;
            userRole.RoleId = newAdmin.GetRoleId();

            // Thêm bảng tham chiếu
            user.UserRoles.Add(userRole);

            // Thêm newUser vào User
            await _userRepository.AddAsync(user);

            // Lưu thay đổi vào database
            await _dbu.SaveChangesAsync();

            return true;
        }
        catch (Exception error)
        {
            Console.Write($@"n{error.Message}");
            return false;
        }
    }

    public async Task<bool> RegisterCustomer(RegisterCustomerDTO newCustomer)
    {
        try
        {
            var checkCustomer = await _userRepository.SingleOrDefaultAsync(customer => customer.Email == newCustomer.Email || customer.UserName == newCustomer.Username);
            if (checkCustomer != null)
            {
                return false;
            }

            User user = new User();
            user.UserName = newCustomer.Username;
            user.FullName = newCustomer.FullName;
            user.Email = newCustomer.Email;
            user.PhoneNumber = newCustomer.PhoneNumber;
            user.PasswordHash = PasswordHelper.HashPassword(newCustomer.Password);
            user.IsDeleted = false;
            user.CreatedAt = DateTime.Now;
            user.DateOfBirth = null;
            user.IsActive = true;

            // Thêm Role vào bảng UserRoles
            UserRole userRole = new UserRole();
            userRole.UserId = user.Id;
            userRole.RoleId = newCustomer.GetRoleId();

            // Thêm bảng tham chiếu
            user.UserRoles.Add(userRole);

            // Thêm newUser vào User
            await _userRepository.AddAsync(user);

            // Lưu thay đổi vào database
            await _dbu.SaveChangesAsync();

            return true;
        }
        catch (Exception error)
        {
            Console.Write($@"n{error.Message}");
            return false;
        }
    }

    public async Task<bool> RequestUpgradeToOwnerAsync(int userId)
    {
        var user = await _userRepository.SingleOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        // Check role qua UserRoles
        var hasCustomerRole = await _userRoleRepository
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == RoleTypeConstDTO.Customer);

        if (!hasCustomerRole)
            return false;

        // Nếu đã có request pending thì không tạo thêm
        var exists = await _upgradeRequestRepository.AnyAsync(r => r.UserId == userId && r.Status == "Pending");
        if (exists) return false;

        UpgradeRequest request = new UpgradeRequest
        {
            UserId = userId,
            Status = "Pending",
            RequestedAt = DateTime.Now
        };

        await _upgradeRequestRepository.AddAsync(request);
        await _dbu.SaveChangesAsync();
        return true;
    }

    public async Task<string> LoginUser(LoginUserDTO userLogin)
    {
        try
        {
            var user = await _userRepository.SingleOrDefaultAsync(user => user.UserName == userLogin.UsernameOrEmail || user.Email == userLogin.UsernameOrEmail);
            if (user == null)
            {
                return MessageLogin.USER_NOT_FOUND;
            }

            // Kiểm tra mật khẩu
            if (PasswordHelper.VerifyPassword(userLogin.Password, user.PasswordHash))
            {
                return _jwtAuthService.GenerateToken(user);
            }
            else
            {
                return MessageLogin.PASSWORD_INCORRECT;
            }
        }
        catch (Exception error)
        {
            return MessageLogin.ERROR_IN_SERVER;
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

            _upgradeRequestRepository.Update(request);


            await _dbu.SaveChangesAsync();
            return true;
        }

        return false;
    }
}