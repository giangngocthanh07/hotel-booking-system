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

        [Fact]
        public async Task RegisterCustomer_UsernameExisted_ShouldReturnConflict()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "existinguser",
                FullName = "Existing User",
                Email = "existing@gmail.com",
                PhoneNumber = "0912345678",
                Password = "ExistingPass@123",
                ConfirmPassword = "ExistingPass@123"
            };

            // Mock validation to succeed
            _mockRegisterCustomerValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Mock user repository to return an existing user with the same username
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, new User
            {
                Id = 2,
                UserName = request.Username,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = request.Password,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsActive = true,
                DateOfBirth = null
            });

            // 2. Act
            var actualResult = await _service.RegisterCustomer(request);

            // 3. Assert
            actualResult.StatusCode.Should().Be(StatusCodeResponse.Conflict);
            actualResult.Message.Should().Be(MessageResponse.UserManagement.Register.USERNAME_EXIST);
            actualResult.Content.Should().BeNull();

            // Verify that AddAsync was never called since registration should fail
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo, 0);
            Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 0);
            Verify_Never_Saved(); // Save should never be called since registration fails at the existence check
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(x => x.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterCustomer_EmailExisted_ShouldReturnConflict()
        {
            // 1. Arrange
            var request = new RegisterCustomerDTO
            {
                Username = "newuser",
                FullName = "New User",
                Email = "new@gmail.com",
                PhoneNumber = "0912345678",
                Password = "NewPass@123",
                ConfirmPassword = "NewPass@123"
            };

            // Mock validation to succeed
            _mockRegisterCustomerValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // Mock user repository to return an existing user with the same email
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, new User
            {
                Id = 2,
                UserName = "differentuser",
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = request.Password,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false,
                IsActive = true,
                DateOfBirth = null
            });

            // 2. Act
            var actualResult = await _service.RegisterCustomer(request);

            // 3. Assert
            actualResult.StatusCode.Should().Be(StatusCodeResponse.Conflict);
            actualResult.Message.Should().Be(MessageResponse.UserManagement.Register.EMAIL_EXIST);
            actualResult.Content.Should().BeNull();

            // Verify that AddAsync was never called since registration should fail
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo, 0);
            Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 0);
            Verify_Never_Saved(); // Save should never be called since registration fails at the existence check
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(x => x.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterCustomer_SaveUserFailed_ShouldRollbackAndReturnError()
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

            // Mock AddAsync for User to throw an exception simulating a database failure during user creation
            _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var actualResult = await _service.RegisterCustomer(request);

            // 3. Assert
            actualResult.StatusCode.Should().Be(StatusCodeResponse.Error);
            actualResult.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            actualResult.Content.Should().BeNull();

            // Verify that AddAsync was called once to add the new user
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo, 0); // UserRole should not be added since User addition failed
            Verify_Saved(1); // Save should be called once for User before the exception occurs
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(x => x.RollBackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterCustomer_SaveUserRoleFailed_ShouldRollbackAndReturnError()
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

            // Mock AddAsync for UserRole to throw an exception
            _mockUnitOfWork.SetupSequence(x => x.SaveChangesAsync())
                .ReturnsAsync(1) // First call for adding User succeeds
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER)); // Second call for adding UserRole fails

            // 2. Act
            var actualResult = await _service.RegisterCustomer(request);

            // 3. Assert
            actualResult.StatusCode.Should().Be(StatusCodeResponse.Error);
            actualResult.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            actualResult.Content.Should().BeNull();

            // Verify that AddAsync was called once to add the new user and once for user role
            _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_AddAsync<IUserRoleRepository, UserRole>(_mockUserRoleRepo);
            Verify_Saved(2); // Save should be called once for User before the exception occurs and once for UserRole which fails
            _mockUnitOfWork.Verify(x => x.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(x => x.RollBackTransactionAsync(), Times.Once);
        }

        // ----- Admin Registration tests -----
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