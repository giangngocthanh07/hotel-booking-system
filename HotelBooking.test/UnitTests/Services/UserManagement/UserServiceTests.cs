using Moq;
using FluentAssertions;
using System.Linq.Expressions;

// 1. Using DTOs and Services from Application layer
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.UserManagement;

// 2. Using Entities and Repo Interfaces from Infrastructure layer
using HotelBooking.infrastructure.Models;
using HotelBooking.application.Services.Domains.UserManagement.Register;
using HotelBooking.application.Services.Domains.UserManagement.Login;

namespace HotelBooking.Tests.Services.UserManagement
{
    public class UserServiceTest : BaseServiceTest
    {
        private readonly Mock<IRegisterService> _mockRegisterService;
        private readonly Mock<ILoginService> _mockLoginService;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly IUserService _service;

        public UserServiceTest()
        {
            _mockRegisterService = new Mock<IRegisterService>();
            _mockLoginService = new Mock<ILoginService>();

            _mockUserRepo = new Mock<IUserRepository>();


            _service = new UserService(_mockRegisterService.Object, _mockLoginService.Object, _mockUserRepo.Object);
        }

        [Fact]
        public async Task GetById_ValidId_ShouldReturnUserDetail()
        {
            // 1. Arrange
            var userId = 1;
            var expectedUser = new User
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

            // Mock DB returning an existing user
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Mock DB returning a user with roles
            _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(expectedUser);

            // 2. Act
            var result = await _service.GetByIdAsync(userId);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
            result.Content.Should().NotBeNull();

            // Verify
            _mockUserRepo.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserRepo.Verify(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }

        [Fact]
        public async Task GetById_InvalidId_ShouldReturnNotFound()
        {
            // 1. Arrange
            var userId = -1;

            // 2. Act
            var result = await _service.GetByIdAsync(userId);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
            result.Message.Should().Be(MessageResponse.Common.INVALID_ID);
            result.Content.Should().BeNull();

            // Verify steps
            _mockUserRepo.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserRepo.Verify(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetById_UserNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            var userId = 1;

            // Mock user not found in DB
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User)null!);

            // 2. Act
            var result = await _service.GetByIdAsync(userId);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.Common.NOT_FOUND);
            result.Content.Should().BeNull();

            // Verify steps
            _mockUserRepo.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserRepo.Verify(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetById_SystemThrowsException_AtGetByIdAsync_ShouldReturnError()
        {
            // 1. Arrange
            var userId = 1;

            // Mock get by id async fail --> FAIL FAST
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.GetByIdAsync(userId);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            result.Content.Should().BeNull();

            // Verify steps
            _mockUserRepo.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserRepo.Verify(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
        }

        [Fact]
        public async Task GetById_SystemThrowsException_AtGetUserWithRoles_ShouldReturnError()
        {
            // 1. Arrange
            var userId = 1;

            // Mock get user with role async fail --> FAIL FAST
            _mockUserRepo.Setup(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.GetByIdAsync(userId);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            result.Content.Should().BeNull();

            // Verify steps
            _mockUserRepo.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mockUserRepo.Verify(r => r.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
        }
    }

}