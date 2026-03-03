using FluentAssertions;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.Services.Domains.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HotelBooking.Tests.Integration.Base;
using HotelBooking.infrastructure.Models;
using Microsoft.Extensions.Configuration;

namespace HotelBooking.Tests.Integration;

/// <summary>
/// INTEGRATION TEST cho UserService.
/// 
/// KHÁC BIỆT LỚN VỚI UNIT TEST:
/// ┌─────────────────────┬──────────────────────────────────────┐
/// │ Unit Test (cũ)      │ Integration Test (file này)          │
/// ├─────────────────────┼──────────────────────────────────────┤
/// │ Mock Repository     │ Repository THẬT (UserRepository)     │
/// │ Mock UnitOfWork     │ UnitOfWork THẬT                      │
/// │ Không có DB         │ SQL Server Docker container THẬT     │
/// │ Test logic service  │ Test cả logic + data layer + FK      │
/// └─────────────────────┴──────────────────────────────────────┘
/// </summary>
public class UserServiceIntegrationTest : IntegrationTestBase
{
    /// <summary>
    /// Tạo UserService THẬT (không mock gì cả).
    /// Mỗi lần gọi sẽ tạo instance mới để đảm bảo clean state.
    /// </summary>
    protected override IServiceProvider BuildServiceProvider(
        HotelBookingDBContext dbContext,
        IConfiguration config)
    {
        return new ServiceCollection()
            .AddTestBase(dbContext, config)
            .AddUserServiceDependencies()
            .BuildServiceProvider();
    }

    private IUserService GetService()
    {
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IUserService>();
    }

    #region RegisterCustomer Integration Tests

    // ==========================================================
    // TEST 1: ĐĂNG KÝ THÀNH CÔNG → VERIFY DATA THẬT TRONG DB
    // ==========================================================
    [Fact]
    public async Task RegisterCustomer_WhenValid_ShouldSaveToDatabase()
    {
        // 1. ARRANGE
        await CleanupDataAsync(); // Xóa data cũ cho sạch
        var service = GetService();

        var input = new RegisterCustomerDTO
        {
            Username = "integration_user",
            Email = "integration@gmail.com",
            Password = "TestPass@123",
            FullName = "Integration Test User",
            PhoneNumber = "0901234567"
        };

        // 2. ACT - Gọi service thật → ghi vào DB thật
        var result = await service.RegisterCustomer(input);

        // 3. ASSERT - Kiểm tra response
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();
        result.Content.IsSuccess.Should().BeTrue();

        // ★ ĐIỂM KHÁC BIỆT LỚN SO VỚI UNIT TEST ★
        // Ở đây ta query TRỰC TIẾP vào DB để verify data đã được lưu thật
        var savedUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == "integration_user");

        savedUser.Should().NotBeNull("User phải được lưu vào DB thật");
        savedUser!.Email.Should().Be("integration@gmail.com");
        savedUser.FullName.Should().Be("Integration Test User");

        // Verify Role cũng đã được gán đúng
        var savedRole = await _dbContext.UserRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(ur => ur.UserId == savedUser.Id);

        savedRole.Should().NotBeNull("UserRole phải được tạo");
        savedRole!.RoleId.Should().Be(RoleTypeConstDTO.Customer, "Phải gán role Customer");
    }

    // ==========================================================
    // TEST 2: ĐĂNG KÝ TRÙNG USERNAME → CONFLICT (DB thật check)
    // ==========================================================
    [Fact]
    public async Task RegisterCustomer_WhenDuplicateUsername_ShouldReturnConflict()
    {
        // 1. ARRANGE - Insert user trước vào DB thật
        await CleanupDataAsync();
        var service = GetService();

        // Tạo user đầu tiên thành công
        var firstUser = new RegisterCustomerDTO
        {
            Username = "trung_ten",
            Email = "first@gmail.com",
            Password = "TestPass@123",
            FullName = "First User",
            PhoneNumber = "0901111111"
        };
        await service.RegisterCustomer(firstUser);

        // 2. ACT - Đăng ký user thứ 2 có CÙNG Username
        var duplicateUser = new RegisterCustomerDTO
        {
            Username = "trung_ten",           // ← TRÙNG!
            Email = "different@gmail.com",    // Email khác
            Password = "TestPass@123",
            FullName = "Duplicate User",
            PhoneNumber = "0902222222"
        };
        var result = await service.RegisterCustomer(duplicateUser);

        // 3. ASSERT
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.UserManagement.Register.USERNAME_EXIST);

        // ★ Verify DB thật: Chỉ có 1 user, không phải 2
        var userCount = await _dbContext.Users.CountAsync();
        userCount.Should().Be(1, "Chỉ user đầu tiên được lưu, user trùng bị chặn");
    }

    // ==========================================================
    // TEST 3: ĐĂNG KÝ TRÙNG EMAIL → CONFLICT (DB thật check)
    // ==========================================================
    [Fact]
    public async Task RegisterCustomer_WhenDuplicateEmail_ShouldReturnConflict()
    {
        // 1. ARRANGE
        await CleanupDataAsync();
        var service = GetService();

        var firstUser = new RegisterCustomerDTO
        {
            Username = "user_one",
            Email = "trung_email@gmail.com",
            Password = "TestPass@123",
            FullName = "First User",
            PhoneNumber = "0903333333"
        };
        await service.RegisterCustomer(firstUser);

        // 2. ACT - Đăng ký user thứ 2 có CÙNG Email
        var duplicateUser = new RegisterCustomerDTO
        {
            Username = "user_two",                 // Username khác
            Email = "trung_email@gmail.com",       // ← TRÙNG!
            Password = "TestPass@123",
            FullName = "Duplicate User",
            PhoneNumber = "0904444444"
        };
        var result = await service.RegisterCustomer(duplicateUser);

        // 3. ASSERT
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.UserManagement.Register.EMAIL_EXIST);

        // ★ Verify DB thật: Chỉ 1 user
        var userCount = await _dbContext.Users.CountAsync();
        userCount.Should().Be(1);
    }

    #endregion

    #region LoginUser Integration Tests

    // ==========================================================
    // TEST 4: ĐĂNG NHẬP THÀNH CÔNG → NHẬN TOKEN THẬT
    // ==========================================================
    [Fact]
    public async Task LoginUser_WhenValid_ShouldReturnToken()
    {
        // 1. ARRANGE - Đăng ký user trước (tạo data thật trong DB)
        await CleanupDataAsync();
        var service = GetService();

        var registerInput = new RegisterCustomerDTO
        {
            Username = "login_test_user",
            Email = "login_test@gmail.com",
            Password = "LoginPass@123",
            FullName = "Login Test User",
            PhoneNumber = "0905555555"
        };
        await service.RegisterCustomer(registerInput);

        // 2. ACT - Login bằng username
        var loginInput = new LoginUserDTO
        {
            UsernameOrEmail = "login_test_user",
            Password = "LoginPass@123"
        };
        var result = await service.LoginUser(loginInput);

        // 3. ASSERT
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();

        // ★ Verify token thật được tạo (không phải mock)
        result.Content.AccessToken.Should().NotBeNullOrEmpty("JWT token phải được generate thật");
        result.Content.FullName.Should().Be("Login Test User");
        result.Content.Roles.Should().Contain("Customer", "User phải có role Customer");
    }

    // ==========================================================
    // TEST 5: ĐĂNG NHẬP SAI PASSWORD → FAIL
    // ==========================================================
    [Fact]
    public async Task LoginUser_WhenWrongPassword_ShouldFail()
    {
        // 1. ARRANGE - Đăng ký user trước
        await CleanupDataAsync();
        var service = GetService();

        var registerInput = new RegisterCustomerDTO
        {
            Username = "wrong_pass_user",
            Email = "wrong_pass@gmail.com",
            Password = "CorrectPass@123",
            FullName = "Wrong Pass User",
            PhoneNumber = "0906666666"
        };
        await service.RegisterCustomer(registerInput);

        // 2. ACT - Login với password SAI
        var loginInput = new LoginUserDTO
        {
            UsernameOrEmail = "wrong_pass_user",
            Password = "WrongPassword@999"       // ← SAI!
        };
        var result = await service.LoginUser(loginInput);

        // 3. ASSERT
        result.Content.Should().BeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Unauthorized);
        result.Message.Should().Be(MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
    }

    #endregion
}
