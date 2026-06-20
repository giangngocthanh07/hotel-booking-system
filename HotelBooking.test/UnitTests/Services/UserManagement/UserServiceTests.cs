using Moq;
using HotelBooking.application.Services.Domains.UserManagement;
using HotelBooking.application.DTOs.User;
using HotelBooking.infrastructure.Models;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.UserManagement.Register;
using HotelBooking.application.Services.Domains.UserManagement.Login;

namespace HotelBooking.test.UnitTests.Services.UserManagement;

public class UserServiceTests : BaseServiceTest<UserService>
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IValidator<UpdateUserProfileDTO>> _mockValidator;
    private readonly Mock<IRegisterService> _mockRegisterService;
    private readonly Mock<ILoginService> _mockLoginService;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockValidator = new Mock<IValidator<UpdateUserProfileDTO>>();
        _mockRegisterService = new Mock<IRegisterService>();
        _mockLoginService = new Mock<ILoginService>();

        _service = new UserService(
            _mockRegisterService.Object,
            _mockLoginService.Object,
            _mockUserRepo.Object,
            _mockLogger.Object,
            _mockValidator.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        int userId = 1;
        var request = new UpdateUserProfileDTO
        {
            FullName = "New Name",
            PhoneNumber = "0123456789"
        };

        var user = new User { Id = userId, UserName = "testuser", Email = "test@example.com" };
        
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        
        // Mock GetByIdAsync logic inside the service which calls GetUserWithRoles
        _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new User { Id = userId, UserName = "testuser", UserRoles = new List<UserRole>() });

        // Act
        var result = await _service.UpdateProfileAsync(userId, request);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_InvalidRequest_ReturnsError()
    {
        // Arrange
        int userId = 1;
        var request = new UpdateUserProfileDTO { FullName = "" };
        var validationFailures = new List<ValidationFailure> { new ValidationFailure("FullName", "Error") };
        
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act
        var result = await _service.UpdateProfileAsync(userId, request);

        // Assert
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidRequest_DoesNotUpdateAvatarUrl()
    {
        // Arrange
        int userId = 1;
        string originalAvatarUrl = "https://example.com/avatar.png";
        var request = new UpdateUserProfileDTO
        {
            FullName = "New Name",
            PhoneNumber = "0123456789",
            DateOfBirth = new DateTime(1990, 1, 1)
        };

        var user = new User 
        { 
            Id = userId, 
            UserName = "testuser", 
            Email = "test@example.com",
            AvatarUrl = originalAvatarUrl
        };
        
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        
        // Mock GetByIdAsync logic inside the service which calls GetUserWithRoles
        _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(new User 
            { 
                Id = userId, 
                UserName = "testuser", 
                AvatarUrl = user.AvatarUrl, // Keep original
                UserRoles = new List<UserRole>() 
            });

        // Act
        var result = await _service.UpdateProfileAsync(userId, request);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(originalAvatarUrl, user.AvatarUrl); // Verify user object kept original AvatarUrl
        Assert.Equal(originalAvatarUrl, result.Content?.AvatarUrl); // Verify returned DTO has original AvatarUrl
        _mockUserRepo.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}

