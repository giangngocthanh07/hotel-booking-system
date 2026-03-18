using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.Services.Domains.UserManagement.Register;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.UserManagement.Register
{
    public class RegisterServiceTests : BaseServiceTest
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<IValidator<RegisterCustomerDTO>> _mockRegisterCustomerValidator;
        private readonly Mock<IValidator<RegisterAdminDTO>> _mockRegisterAdminValidator;
        private readonly IRegisterService _service;
        public RegisterServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockRegisterCustomerValidator = new Mock<IValidator<RegisterCustomerDTO>>();
            _mockRegisterAdminValidator = new Mock<IValidator<RegisterAdminDTO>>();


            _service = new RegisterService(_mockUserRepo.Object, _mockUserRoleRepo.Object, _mockRegisterCustomerValidator.Object, _mockRegisterAdminValidator.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task RegisterCustomer_ValidRequest_ShouldReturnSuccess()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "testuser",
                FullName = "Test User",
                Email = "test@gmail.com",
                PhoneNumber = "0912345678",
                Password = "TestPass@123",
                ConfirmPassword = "TestPass@123"
            };

            // Mock validation to succeed
            _mockRegisterCustomerValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Mock user repository to return null (no existing user with same email/username)
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            // 2. Act
            var actualResult = await _service.RegisterCustomer(request);

            // 3. Assert
            actualResult.StatusCode.Should().Be(StatusCodeResponse.Success);
            actualResult.Message.Should().Be(MessageResponse.UserManagement.Register.SUCCESS);
            actualResult.Content.Should().NotBeNull();
            actualResult.Content.Email.Should().Be(request.Email);
            actualResult.Content.FullName.Should().Be(request.FullName);
            actualResult.Content.Username.Should().Be(request.Username);

            // Verify that AddAsync was called once to add the new user
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
            Verify_Saved(2); // Expecting 2 saves: one for User and one for UserRole
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollBackTransactionAsync(), Times.Never);
        }


        // Admin Registration tests
        [Fact]
        public async Task RegisterAdmin_ValidRequest_ShouldReturnSuccess()
        {
            // 1. Arrange
            var request = new RegisterAdminDTO
            {
                Username = "adminuser",
                FullName = "Admin User",
                Email = "admin@gmail.com",
                PhoneNumber = "0912345678",
                Password = "AdminPass@123",
                ConfirmPassword = "AdminPass@123"
            };


            // 2. Act
            // Mock validation to succeed
            _mockRegisterAdminValidator.Setup(x => x.ValidateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Mock user repository to return null (no existing user with same email/username)
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            var actualResult = await _service.RegisterAdmin(request);

            // 3. Assert
            actualResult.StatusCode.Should().Be(StatusCodeResponse.Success);
            actualResult.Message.Should().Be(MessageResponse.UserManagement.Register.SUCCESS);
            actualResult.Content.Should().NotBeNull();
            actualResult.Content.Email.Should().Be(request.Email);
            actualResult.Content.FullName.Should().Be(request.FullName);
            actualResult.Content.Username.Should().Be(request.Username);

            // Verify that AddAsync was called once to add the new admin user
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
            Verify_Saved(2); // Expecting 2 saves: one for User and one for UserRole
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(x => x.RollBackTransactionAsync(), Times.Never);

        }
    }
}