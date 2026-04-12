using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.DTOs.Role;

using HotelBooking.infrastructure.Models;

using HotelBooking.Tests.Integration.Base;
using HotelBooking.application.Services.Domains.UserManagement.Register;
using HotelBooking.application.Services.Domains.UserManagement.Login;

namespace HotelBooking.Tests.Integration;

/// <summary>
/// INTEGRATION TESTS for UserService.
///
/// KEY DIFFERENCES FROM UNIT TESTS:
/// ┌─────────────────────┬──────────────────────────────────────┐
/// │ Unit Test           │ Integration Test (this file)         │
/// ├─────────────────────┼──────────────────────────────────────┤
/// │ Mock Repository     │ Real Repository (UserRepository)     │
/// │ Mock UnitOfWork     │ Real UnitOfWork                      │
/// │ No DB               │ Real SQL Server Docker container     │
/// │ Tests service logic │ Tests logic + data layer + FK        │
/// └─────────────────────┴──────────────────────────────────────┘
/// </summary>
public class UserServiceIntegrationTest : IntegrationTestBase
{
    /// <summary>
    /// Creates a real UserService instance (no mocks).
    /// Each call creates a new instance to ensure a clean state.
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

    private IRegisterService GetRegisterService()
    {
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IRegisterService>();
    }

    private ILoginService GetLoginService()
    {
        var scope = _serviceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ILoginService>();
    }

    #region RegisterCustomer Integration Tests

    // ============================================================
    // TEST 1: REGISTER SUCCESS → VERIFY REAL DATA IN DB
    // ============================================================
    [Fact]
    public async Task RegisterCustomer_WhenValid_ShouldSaveToDatabase()
    {
        // 1. ARRANGE
        await CleanupDataAsync(); // Clear existing data for a clean state
        var service = GetRegisterService();

        var input = new RegisterCustomerDTO
        {
            Username = "integration_user",
            Email = "integration@gmail.com",
            Password = "TestPass@123",
            FullName = "Integration Test User",
            PhoneNumber = "0901234567"
        };

        // 2. ACT - Call the real service → writes to real DB
        var result = await service.RegisterCustomer(input);

        // 3. ASSERT - Verify the response
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();

        // ★ KEY DIFFERENCE FROM UNIT TEST ★
        // Here we query DIRECTLY into DB to verify data was truly persisted
        var savedUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == "integration_user");

        savedUser.Should().NotBeNull("User must be saved to the real DB");
        savedUser!.Email.Should().Be("integration@gmail.com");
        savedUser.FullName.Should().Be("Integration Test User");

        // Verify Role was correctly assigned
        var savedRole = await _dbContext.UserRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(ur => ur.UserId == savedUser.Id);

        savedRole.Should().NotBeNull("UserRole must be created");
        savedRole!.RoleId.Should().Be(RoleTypeConstDTO.Customer, "Must be assigned Customer role");
    }

    // ============================================================
    // TEST 2: REGISTER DUPLICATE USERNAME → CONFLICT (real DB check)
    // ============================================================
    [Fact]
    public async Task RegisterCustomer_WhenDuplicateUsername_ShouldReturnConflict()
    {
        // 1. ARRANGE - Insert user first into real DB
        await CleanupDataAsync();
        var service = GetRegisterService();

        // Register the first user successfully
        var firstUser = new RegisterCustomerDTO
        {
            Username = "trung_ten",
            Email = "first@gmail.com",
            Password = "TestPass@123",
            FullName = "First User",
            PhoneNumber = "0901111111"
        };
        await service.RegisterCustomer(firstUser);

        // 2. ACT - Register a 2nd user with the SAME Username
        var duplicateUser = new RegisterCustomerDTO
        {
            Username = "trung_ten",           // ← DUPLICATE!
            Email = "different@gmail.com",    // Email khác
            Password = "TestPass@123",
            FullName = "Duplicate User",
            PhoneNumber = "0902222222"
        };
        var result = await service.RegisterCustomer(duplicateUser);

        // 3. ASSERT
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.UserManagement.Register.USERNAME_EXIST);

        // ★ Verify real DB: Only 1 user, not 2
        var userCount = await _dbContext.Users.CountAsync();
        userCount.Should().Be(1, "Only the first user is saved; the duplicate is blocked");
    }

    // ============================================================
    // TEST 3: REGISTER DUPLICATE EMAIL → CONFLICT (real DB check)
    // ============================================================
    [Fact]
    public async Task RegisterCustomer_WhenDuplicateEmail_ShouldReturnConflict()
    {
        // 1. ARRANGE
        await CleanupDataAsync();
        var service = GetRegisterService();

        var firstUser = new RegisterCustomerDTO
        {
            Username = "user_one",
            Email = "trung_email@gmail.com",
            Password = "TestPass@123",
            FullName = "First User",
            PhoneNumber = "0903333333"
        };
        await service.RegisterCustomer(firstUser);

        // 2. ACT - Register 2nd user with the SAME Email
        var duplicateUser = new RegisterCustomerDTO
        {
            Username = "user_two",                 // Username khác
            Email = "trung_email@gmail.com",       // ← DUPLICATE!
            Password = "TestPass@123",
            FullName = "Duplicate User",
            PhoneNumber = "0904444444"
        };
        var result = await service.RegisterCustomer(duplicateUser);

        // 3. ASSERT
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.UserManagement.Register.EMAIL_EXIST);

        // ★ Verify real DB: Only 1 user
        var userCount = await _dbContext.Users.CountAsync();
        userCount.Should().Be(1);
    }

    #endregion

    #region LoginUser Integration Tests

    // ============================================================
    // TEST 4: LOGIN SUCCESS → RECEIVE REAL TOKEN
    // ============================================================
    [Fact]
    public async Task LoginUser_WhenValid_ShouldReturnToken()
    {
        // 1. ARRANGE - Register user first (create real data in DB)
        await CleanupDataAsync();
        var registerService = GetRegisterService();
        var loginService = GetLoginService();

        var registerInput = new RegisterCustomerDTO
        {
            Username = "login_test_user",
            Email = "login_test@gmail.com",
            Password = "LoginPass@123",
            FullName = "Login Test User",
            PhoneNumber = "0905555555"
        };
        await registerService.RegisterCustomer(registerInput);

        // 2. ACT - Login by username
        var loginInput = new LoginUserDTO
        {
            UsernameOrEmail = "login_test_user",
            Password = "LoginPass@123"
        };
        var result = await loginService.LoginUser(loginInput);

        // 3. ASSERT
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Content.Should().NotBeNull();

        // ★ Verify a real token is generated (not a mock)
        result.Content.AccessToken.Should().NotBeNullOrEmpty("JWT token must be genuinely generated");
        result.Content.FullName.Should().Be("Login Test User");
        result.Content.Roles.Should().Contain("Customer", "User must have Customer role");
    }

    // ============================================================
    // TEST 5: LOGIN WITH WRONG PASSWORD → FAIL
    // ============================================================
    [Fact]
    public async Task LoginUser_WhenWrongPassword_ShouldFail()
    {
        // 1. ARRANGE - Register user first
        await CleanupDataAsync();
        var registerService = GetRegisterService();
        var loginService = GetLoginService();

        var registerInput = new RegisterCustomerDTO
        {
            Username = "wrong_pass_user",
            Email = "wrong_pass@gmail.com",
            Password = "CorrectPass@123",
            FullName = "Wrong Pass User",
            PhoneNumber = "0906666666"
        };
        await registerService.RegisterCustomer(registerInput);

        // 2. ACT - Login with WRONG password
        var loginInput = new LoginUserDTO
        {
            UsernameOrEmail = "wrong_pass_user",
            Password = "WrongPassword@999"       // ← WRONG!
        };
        var result = await loginService.LoginUser(loginInput);

        // 3. ASSERT
        result.Content.Should().BeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Unauthorized);
        result.Message.Should().Be(MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
    }

    #endregion
}
