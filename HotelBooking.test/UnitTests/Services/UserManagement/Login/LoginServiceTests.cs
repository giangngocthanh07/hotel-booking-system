using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.Services.Domains.Auth;
using HotelBooking.application.Services.Domains.UserManagement.Login;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.UserManagement.Login;

public class LoginServiceTests : BaseServiceTest<LoginService>
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IValidator<LoginUserDTO>> _mockLoginValidator;
    private readonly Mock<IJwtAuthService> _mockJwtAuthService;
    private readonly ILoginService _service;
    public LoginServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockLoginValidator = new Mock<IValidator<LoginUserDTO>>();

        _mockJwtAuthService = new Mock<IJwtAuthService>();

        _service = new LoginService(_mockUserRepo.Object, _mockJwtAuthService.Object, _mockLoginValidator.Object, _mockLogger.Object);

    }

    [Fact]
    public async Task LoginUser_ValidRequest_ShouldSuccess()
    {
        // 1. ARRANGE
        var input = new LoginUserDTO
        {
            UsernameOrEmail = "testuser",
            Password = "ValidPass@123"
        };

        _mockLoginValidator.Setup(v => v.Validate(input))
        .Returns(new FluentValidation.Results.ValidationResult());

        var mockToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.MockToken";
        _mockJwtAuthService.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(mockToken);

        // Mock DB returning a user with roles
        var user = new User
        {
            Id = 1,
            UserName = "testuser",
            Email = "test@gmail.com",
            PasswordHash = PasswordHelper.HashPassword("ValidPass@123"),
            FullName = "Test User",
            UserRoles = new List<UserRole>
                {
                    new UserRole { RoleId = RoleTypeConstDTO.Customer, Role = new Role { Name = "Customer" } }
                }
        };

        _mockUserRepo.Setup(x => x.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        // 2. ACT
        var result = await _service.LoginUser(input);

        // 3. ASSERT
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.UserManagement.Login.SUCCESS);
        result.Content.Should().NotBeNull();
        result.Content.AccessToken.Should().Be(mockToken);
    }

    [Fact]
    public async Task LoginUser_InvalidRequest_ShouldReturnBadRequest()
    {
        // 1. Arrange
        var request = new LoginUserDTO { UsernameOrEmail = "", Password = "" };

        var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("UsernameOrEmail", "Username or Email is required")
            };

        _mockLoginValidator.Setup(v => v.Validate(request))
            .Returns(new FluentValidation.Results.ValidationResult(validationFailures));

        // 2. Act
        var result = await _service.LoginUser(request);

        // 3. Assert
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailures.First().ErrorMessage);
        result.Content.Should().BeNull();

        // Verify
        _mockUserRepo.Verify(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
    }

    [Fact]
    public async Task LoginUser_UserNotFound_ShouldReturnNotFound()
    {
        // 1. Arrange
        var request = new LoginUserDTO { UsernameOrEmail = "notfound", Password = "123" };

        _mockLoginValidator.Setup(v => v.Validate(request))
            .Returns(new FluentValidation.Results.ValidationResult());



        // Mock user not found in DB
        _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User)null!);

        // 2. Act
        var result = await _service.LoginUser(request);

        // 3. Assert
        result.StatusCode.Should().Be(StatusCodeResponse.Unauthorized);
        result.Message.Should().Be(MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
        result.Content.Should().BeNull();
    }

    [Fact]
    public async Task LoginUser_WrongPassword_ShouldReturnUnauthorized()
    {
        // 1. Arrange
        var request = new LoginUserDTO { UsernameOrEmail = "testuser", Password = "WrongPassword" };

        _mockLoginValidator.Setup(v => v.Validate(request))
            .Returns(new FluentValidation.Results.ValidationResult());

        var existingUser = new User
        {
            UserName = "testuser",
            // A password is different from the WrongPassword
            PasswordHash = PasswordHelper.HashPassword("CorrectPassword@123")
        };

        _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(existingUser);

        // 2. Act
        var result = await _service.LoginUser(request);

        // 3. Assert
        result.StatusCode.Should().Be(StatusCodeResponse.Unauthorized);
        result.Message.Should().Be(MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
        result.Content.Should().BeNull();

        // Verify
        _mockUserRepo.Verify(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        _mockJwtAuthService.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginUser_SystemThrowsException_ShouldReturnError()
    {
        // 1. Arrange
        var request = new LoginUserDTO { UsernameOrEmail = "testuser", Password = "123" };

        _mockLoginValidator.Setup(v => v.Validate(request))
            .Returns(new FluentValidation.Results.ValidationResult());

        _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.LoginUser(request);

        // 3. Assert
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.UserManagement.Login.ERROR_IN_SERVER);
        result.Content.Should().BeNull();
        VerifyLogErrorOnce();
    }

}