using Moq;
using FluentAssertions;
using FluentValidation;
using System.Linq.Expressions;

// 1. Using DTOs and Services from Application layer
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.DTOs.User.Login;
using HotelBooking.application.DTOs.Role;
using HotelBooking.application.Validators.UserManagement.Register;
using HotelBooking.application.Validators.UserManagement.Login;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.UserManagement;

// 2. Using Entities and Repo Interfaces from Infrastructure layer
using HotelBooking.infrastructure.Models;

namespace HotelBooking.Tests.Services
{
    public class UserServiceTest : BaseServiceTest
    {
        // Repository Mocks
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<IUpgradeRequestRepository> _mockUpgradeRepo;

        // Validator Mocks
        private readonly Mock<IValidator<RegisterCustomerDTO>> _mockRegisterCustomerValidator;
        private readonly Mock<IValidator<RegisterAdminDTO>> _mockRegisterAdminValidator;
        private readonly Mock<IValidator<LoginUserDTO>> _mockLoginValidator;

        // Service under test
        private readonly UserService _userService;

        public UserServiceTest()
        {
            // Initialize Mocks
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockUpgradeRepo = new Mock<IUpgradeRequestRepository>();

            // Initialize Validator Mocks (using real validators to maintain behavioral integrity)
            _mockRegisterCustomerValidator = new Mock<IValidator<RegisterCustomerDTO>>();
            _mockRegisterCustomerValidator.Setup(v => v.Validate(It.IsAny<RegisterCustomerDTO>()))
                .Returns((RegisterCustomerDTO dto) => new RegisterCustomerValidator().Validate(dto));

            _mockRegisterAdminValidator = new Mock<IValidator<RegisterAdminDTO>>();
            _mockRegisterAdminValidator.Setup(v => v.Validate(It.IsAny<RegisterAdminDTO>()))
                .Returns((RegisterAdminDTO dto) => new RegisterAdminValidator().Validate(dto));

            _mockLoginValidator = new Mock<IValidator<LoginUserDTO>>();
            _mockLoginValidator.Setup(v => v.Validate(It.IsAny<LoginUserDTO>()))
                .Returns((LoginUserDTO dto) => new LoginValidator().Validate(dto));

            // Initialize Service with all dependencies
            // Note: The order must strictly match the constructor in UserService.cs
            _userService = new UserService(
                _mockUserRepo.Object,           // 1. UserRepository
                _mockUserRoleRepo.Object,       // 2. UserRoleRepository
                _mockUpgradeRepo.Object,        // 3. UpgradeRequestRepository
                null!,                          // 4. JwtAuthService (Not required for registration)
                _mockUnitOfWork.Object,         // 5. UnitOfWork (Inherited from Base)
                _mockRegisterCustomerValidator.Object,
                _mockRegisterAdminValidator.Object,
                _mockLoginValidator.Object
            );
        }

        #region RegisterCustomer Tests
        // ==========================================================
        // TEST CASE 1: SUCCESSFUL REGISTRATION (HAPPY PATH)
        // ==========================================================
        [Fact]
        public async Task RegisterCustomer_WhenInfoValid_ShouldSuccess()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "new_user",
                Email = "new@gmail.com",
                Password = "TestPassword@123",
                FullName = "New User"
            };

            // Using Generic Helper from Base class
            // Mock DB returning null (meaning Username/Email are available)
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT (Using Fluent Assertions)
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageRegister.REGISTER_SUCCESS);
            result.Content.Should().NotBeNull();
            result.Content.IsSuccess.Should().BeTrue();

            // Verify AddAsync was called exactly once
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo, 1);

            // Verify SaveChangesAsync was called twice (once for User, once for Role assignment)
            Verify_Saved(2);
        }

        // ==========================================================
        // TEST CASE 2: DUPLICATE USERNAME ERROR
        // ==========================================================
        [Fact]
        public async Task RegisterCustomer_WhenUsernameExists_ShouldReturnConflict()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "existing_user",
                Email = "new@gmail.com",
                Password = "TestPassword@123",
                FullName = "User Test"
            };

            // Mock DB finding an existing user with the same username
            var existingUser = new User { UserName = "existing_user", Email = "old@gmail.com" };
            MockRepo_Find_Returns(_mockUserRepo, existingUser);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageRegister.USERNAME_EXIST);
            result.StatusCode.Should().Be(StatusCodeResponse.Conflict);

            // Ensure AddAsync is NEVER called
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 3: DUPLICATE EMAIL ERROR
        // ==========================================================
        [Fact]
        public async Task RegisterCustomer_WhenEmailExists_ShouldReturnConflict()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "unique_user",
                Email = "duplicate@gmail.com",
                Password = "TestPassword@123",
                FullName = "User Test"
            };

            // Mock DB finding an existing user with the same email
            var existingUser = new User { UserName = "old_user", Email = "duplicate@gmail.com" };
            MockRepo_Find_Returns(_mockUserRepo, existingUser);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageRegister.EMAIL_EXIST);
            result.StatusCode.Should().Be(StatusCodeResponse.Conflict);

            // Ensure AddAsync is NEVER called
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 4: SYSTEM EXCEPTION (SERVER ERROR)
        // ==========================================================
        [Fact]
        public async Task RegisterCustomer_WhenExceptionOccurs_ShouldReturnError()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "user",
                Email = "email@gmail.com",
                Password = "TestPassword@123",
                FullName = "User Test"
            };

            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            // Simulate database crash during AddAsync
            MockRepo_Add_ThrowsException<IUserRepository, User>(_mockUserRepo);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.ERROR_IN_SERVER);
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
        }

        // ==========================================================
        // TEST CASE 5: INVALID EMAIL FORMAT
        // ==========================================================
        [Fact]
        public async Task RegisterCustomer_WhenEmailFormatIsInvalid_ShouldReturnBadRequest()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "user_test",
                Email = "invalid_email_format", // No @ or domain
                Password = "TestPassword@123",
                FullName = "Test User"
            };

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageRegister.INVALID_EMAIL);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            // Ensure NO database calls were made due to early validation failure
            Verify_Repo_Never_SingleOrDefaultAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 6: INVALID PASSWORD (MULTIPLE SCENARIOS)
        // ==========================================================
        [Theory]
        // Empty/Null Scenarios
        [InlineData("", MessageResponse.UserManagement.Register.EMPTY_PASSWORD)]
        [InlineData(null, MessageResponse.UserManagement.Register.EMPTY_PASSWORD)]
        [InlineData("   ", MessageResponse.UserManagement.Register.EMPTY_PASSWORD)]

        // Format Scenarios
        [InlineData("short", MessageResponse.UserManagement.Register.SHORT_PASSWORD)]
        [InlineData("nocapital1@", MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD)]
        [InlineData("NO_LOWER_123", MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD)]
        [InlineData("NoSpecialChar1", MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD)]
        public async Task RegisterCustomer_WhenPasswordIsInvalid_ShouldReturnBadRequest(string? invalidPassword, string expectedMsg)
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "user_test",
                Email = "test@gmail.com",
                Password = invalidPassword!,
                FullName = "Test User"
            };

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(expectedMsg);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            // Ensure NO database calls were made
            Verify_Repo_Never_SingleOrDefaultAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }
        #endregion

        #region RegisterAdmin Tests
        // ==========================================================
        // TEST CASE 1: SUCCESSFUL ADMIN REGISTRATION
        // ==========================================================
        [Fact]
        public async Task RegisterAdmin_WhenInfoValid_ShouldSuccess()
        {
            // 1. ARRANGE
            var input = new RegisterAdminDTO
            {
                Username = "admin_new",
                Email = "admin@gmail.com",
                Password = "AdminPass@123",
                FullName = "Admin User",
                PhoneNumber = "0912345678"
            };

            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            // 2. ACT
            var result = await _userService.RegisterAdmin(input);

            // 3. ASSERT
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageResponse.UserManagement.Register.SUCCESS);
            result.Content.IsSuccess.Should().BeTrue();

            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo, 1);
            Verify_Saved(2);
        }
        #endregion

        #region LoginUser Tests
        // ==========================================================
        // TEST CASE 1: SUCCESSFUL LOGIN (HAPPY PATH)
        // ==========================================================
        [Fact]
        public async Task LoginUser_WhenCredentialsValid_ShouldSuccess()
        {
            // 1. ARRANGE
            var input = new LoginUserDTO
            {
                UsernameOrEmail = "testuser",
                Password = "ValidPass@123"
            };

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
            var result = await _userService.LoginUser(input);

            // 3. ASSERT
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageResponse.UserManagement.Login.SUCCESS);
            result.Content.Should().NotBeNull();
            result.Content.AccessToken.Should().NotBeNullOrEmpty();
            result.Content.FullName.Should().Be("Test User");
            result.Content.Roles.Should().Contain("Customer");
        }

        // ==========================================================
        // TEST CASE 2: LOGIN FAILURE - USER NOT FOUND
        // ==========================================================
        [Fact]
        public async Task LoginUser_WhenUserNotFound_ShouldReturnNotFound()
        {
            // 1. ARRANGE
            var input = new LoginUserDTO { UsernameOrEmail = "ghost", Password = "any" };

            _mockUserRepo.Setup(x => x.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync((User?)null);

            // 2. ACT
            var result = await _userService.LoginUser(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
        }
        #endregion
    }
}