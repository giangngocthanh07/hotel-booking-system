using Moq;
using FluentAssertions;

// 1. Using DTO và Service từ tầng Application
using HotelBooking.application.Services.Domains.UserManagement; // Nơi chứa UserService
using HotelBooking.application.Validators.UserManagement.Register; // Nơi chứa RegisterCustomerDTO, RegisterAdminDTO
using HotelBooking.application.Validators.UserManagement.Login; // Nơi chứa LoginUserDTO

// 2. Using Entity và Interface Repo từ tầng Infrastructure
using HotelBooking.infrastructure.Models;
using FluentValidation;
using HotelBooking.application.Helpers;
using System.Linq.Expressions;
using HotelBooking.application.DTOs.User.Register;
using HotelBooking.application.DTOs.User.Login; // Nơi chứa class User, Room...

namespace HotelBooking.Tests.Services
{
    public class UserServiceTest : BaseServiceTest
    {
        // Khai báo các Mock cần thiết
        private readonly Mock<IUserRepository> _mockUserRepo;

        // Khai báo thêm các Mock còn thiếu theo đúng Constructor của bạn
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<IUpgradeRequestRepository> _mockUpgradeRepo;

        // Validators mocks
        private readonly Mock<IValidator<RegisterCustomerDTO>> _mockRegisterCustomerValidator;
        private readonly Mock<IValidator<RegisterAdminDTO>> _mockRegisterAdminValidator;
        private readonly Mock<IValidator<LoginUserDTO>> _mockLoginValidator;

        // Service cần test
        private readonly UserService _userService;

        public UserServiceTest()
        {
            // 2. Khởi tạo các Mock
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockUpgradeRepo = new Mock<IUpgradeRequestRepository>();

            // 3. Khởi tạo các Mock validator (mặc định dùng validator thật để giữ behavior)
            _mockRegisterCustomerValidator = new Mock<IValidator<RegisterCustomerDTO>>();
            _mockRegisterCustomerValidator.Setup(v => v.Validate(It.IsAny<RegisterCustomerDTO>()))
                .Returns((RegisterCustomerDTO dto) => new RegisterCustomerValidator().Validate(dto));

            _mockRegisterAdminValidator = new Mock<IValidator<RegisterAdminDTO>>();
            _mockRegisterAdminValidator.Setup(v => v.Validate(It.IsAny<RegisterAdminDTO>()))
                .Returns((RegisterAdminDTO dto) => new RegisterAdminValidator().Validate(dto));

            _mockLoginValidator = new Mock<IValidator<LoginUserDTO>>();
            _mockLoginValidator.Setup(v => v.Validate(It.IsAny<LoginUserDTO>()))
                .Returns((LoginUserDTO dto) => new LoginValidator().Validate(dto));

            // 4. Khởi tạo Service với đầy đủ tham số (thêm 3 validator ở cuối)
            // Thứ tự phải CHÍNH XÁC như trong file UserService.cs
            _userService = new UserService(
                _mockUserRepo.Object,       // 1. UserRepository (Cái chính ta dùng)
                _mockUserRoleRepo.Object,   // 2. UserRoleRepository (Mock thêm cho đủ tụ)
                _mockUpgradeRepo.Object,    // 4. UpgradeRequestRepository (Mock thêm cho đủ tụ)
                null!,                       // 5. JwtAuthService (Hàm Register ko dùng Token -> null)
                _mockUnitOfWork.Object,     // 6. UnitOfWork
                _mockRegisterCustomerValidator.Object,
                _mockRegisterAdminValidator.Object,
                _mockLoginValidator.Object
            );
        }

        #region RegisterCustomer Tests
        // ==========================================================
        // TEST CASE 1: ĐĂNG KÝ THÀNH CÔNG (HAPPY PATH)
        // ==========================================================
        [Fact]
        [Obsolete]
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

            // 2. Dùng Helper Generic của cha
            // Giả lập DB trả về null (nghĩa là chưa có ai trùng Username hay Email cả)
            // Truyền vào Mock Repo User và kết quả mong muốn (null)
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            // (Lưu ý: Không cần setup SaveChangesAsync nữa vì Base Test làm rồi)
            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT VỚI FLUENT ASSERTIONS
            // Đọc như văn nói: "Result StatusCode nên là Success"
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

            // Check message
            result.Message.Should().Be(MessageRegister.REGISTER_SUCCESS);

            // 1. Check null trước
            result.Content.Should().NotBeNull();

            // 2. IsSuccess phải là true
            result.Content.IsSuccess.Should().BeTrue();

            // Quan trọng: Kiểm tra xem hàm AddAsync có được gọi đúng 1 lần không?
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo, 1);

            // Kiểm tra SaveChangesAsync có được gọi 2 lần không? 
            // (Trong code của bạn gọi 2 lần: 1 lần lưu User, 1 lần lưu Role)
            Verify_Saved(2);
        }

        // ==========================================================
        // TEST CASE 2: LỖI TRÙNG USERNAME
        // ==========================================================
        [Fact]
        [Obsolete]
        public async Task RegisterCustomer_WhenUsernameExists_ShouldReturnConflict()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "trung_ten",
                Email = "new@gmail.com",
                Password = "TestPassword@123", // <--- QUAN TRỌNG: Phải thêm dòng này!
                FullName = "User Test"
            };

            // 2. Dùng Helper Generic của cha
            // Giả lập DB tìm thấy 1 thằng trùng Username
            // Truyền vào Mock Repo User và kết quả mong muốn (null)
            var existingUser = new User { UserName = "trung_ten", Email = "old@gmail.com" };

            MockRepo_Find_Returns(_mockUserRepo, existingUser);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. FLUENTASSERTIONS
            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageRegister.USERNAME_EXIST);
            result.StatusCode.Should().Be(StatusCodeResponse.Conflict);

            // Đảm bảo KHÔNG bao giờ gọi hàm lưu
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 3: LỖI TRÙNG EMAIL
        // ==========================================================
        [Fact]
        [Obsolete]
        public async Task RegisterCustomer_WhenEmailExists_ShouldReturnConflict()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "khac_ten",
                Email = "trung_email@gmail.com",
                Password = "TestPassword@123", // <--- QUAN TRỌNG: Phải thêm dòng này!
                FullName = "User Test"
            };

            // 2. Dùng Helper Generic của cha
            // Giả lập DB tìm thấy 1 thằng trùng Email
            // Truyền vào Mock Repo User và kết quả mong muốn (null)
            var existingUser = new User { UserName = "old_user", Email = "trung_email@gmail.com" };

            MockRepo_Find_Returns(_mockUserRepo, existingUser);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. FLUENTASSERTIONS
            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageRegister.EMAIL_EXIST);    // Check đúng thông báo trùng email
            result.StatusCode.Should().Be(StatusCodeResponse.Conflict);

            // Đảm bảo KHÔNG bao giờ gọi hàm lưu
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 4: LỖI HỆ THỐNG (EXCEPTION)
        // ==========================================================
        [Fact]
        [Obsolete]
        public async Task RegisterCustomer_WhenExceptionOccurs_ShouldReturnError()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "user",
                Email = "email@gmail.com",
                Password = "TestPassword@123", // <--- QUAN TRỌNG: Phải thêm dòng này!
                FullName = "User Test"
            };

            // 2. Dùng Helper Generic của cha
            // Giả lập chưa trùng ai cả
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            // NHƯNG khi gọi hàm AddAsync thì DB bị sập (Throw Exception)
            MockRepo_Add_ThrowsException<IUserRepository, User>(_mockUserRepo);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. FLUENTASSERTIONS
            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.ERROR_IN_SERVER);
            result.StatusCode.Should().Be(StatusCodeResponse.Error);

        }

        // ==========================================================
        // TEST CASE 5: EMAIL SAI ĐỊNH DẠNG (INVALID FORMAT)
        // ==========================================================
        [Fact]
        [Obsolete]
        public async Task RegisterCustomer_WhenEmailFormatIsInvalid_ShouldReturnBadRequest()
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "user_test",
                Email = "email_nay_bi_sai", // <--- KHÔNG CÓ @ và domain
                Password = "TestPassword@123",
                FullName = "Test User"
            };

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageRegister.INVALID_EMAIL);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            // QUAN TRỌNG NHẤT:
            // Vì email sai ngay từ vòng gửi xe, nên KHÔNG ĐƯỢC PHÉP gọi xuống Database
            Verify_Repo_Never_SingleOrDefaultAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 6: PASSWORD KHÔNG HỢP LỆ (INVALID)
        // ==========================================================
        [Theory]
        // --- Nhóm Rỗng/Null (Cái bạn vừa yêu cầu) ---
        [InlineData("", MessageResponse.UserManagement.Register.EMPTY_PASSWORD)]          // Rỗng tuyệt đối
        [InlineData(null, MessageResponse.UserManagement.Register.EMPTY_PASSWORD)]        // Null
        [InlineData("   ", MessageResponse.UserManagement.Register.EMPTY_PASSWORD)]       // Chỉ có khoảng trắng (Space)

        // --- Nhóm Sai Định Dạng (Cái cũ) ---
        [InlineData("short", MessageResponse.UserManagement.Register.SHORT_PASSWORD)]           // Quá ngắn (< 8 ký tự)
        [InlineData("nocapital1@", MessageResponse.UserManagement.Register.UPPERCASE_LETTER_PASSWORD)]     // Thiếu chữ hoa
        [InlineData("NO_LOWER_123", MessageResponse.UserManagement.Register.LOWERCASE_LETTER_PASSWORD)]    // Thiếu chữ thường
        [InlineData("NoSpecialChar1", MessageResponse.UserManagement.Register.SPECIAL_CHARACTER_PASSWORD)]  // Thiếu ký tự đặc biệt
        [Obsolete]
        public async Task RegisterCustomer_WhenPasswordIsInvalid_ShouldReturnBadRequest(string? invalidPassword, string expectedMsg)
        {
            // 1. ARRANGE
            var input = new RegisterCustomerDTO
            {
                Username = "user_test",
                Email = "email_test@gmail.com", // <--- KHÔNG CÓ @ và domain
                Password = invalidPassword!, // Truyền cái pass "dỏm" vào đây
                FullName = "Test User"
            };

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            // QUAN TRỌNG: Giờ ta check chính xác câu thông báo lỗi
            result.Message.Should().Be(expectedMsg);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            // QUAN TRỌNG NHẤT:
            // Vì password rỗng ngay từ vòng gửi xe, nên KHÔNG ĐƯỢC PHÉP gọi xuống Database
            Verify_Repo_Never_SingleOrDefaultAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }
        #endregion

        #region RegisterAdmin Tests
        // ==========================================================
        // TEST CASE 1: ĐĂNG KÝ ADMIN THÀNH CÔNG (HAPPY PATH)
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

            // Giả lập DB chưa có ai trùng Username hay Email
            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);

            // 2. ACT
            var result = await _userService.RegisterAdmin(input);

            // 3. ASSERT
            result.StatusCode.Should().Be(StatusCodeResponse.Success);
            result.Message.Should().Be(MessageResponse.UserManagement.Register.SUCCESS);
            result.Content.Should().NotBeNull();
            result.Content.IsSuccess.Should().BeTrue();

            // Verify hàm AddAsync gọi 1 lần
            Verify_Repo_AddAsync<IUserRepository, User>(_mockUserRepo, 1);
            // Verify SaveChangesAsync gọi 2 lần (1 lần lưu User, 1 lần lưu Role)
            Verify_Saved(2);
        }

        // ==========================================================
        // TEST CASE 2: ADMIN - LỖI TRÙNG USERNAME
        // ==========================================================
        [Fact]
        public async Task RegisterAdmin_WhenUsernameExists_ShouldReturnConflict()
        {
            // 1. ARRANGE
            var input = new RegisterAdminDTO
            {
                Username = "admin_trung",
                Email = "new_admin@gmail.com",
                Password = "AdminPass@123",
                FullName = "Admin Test",
                PhoneNumber = "0912345678"
            };

            var existingAdmin = new User { UserName = "admin_trung", Email = "old_admin@gmail.com" };
            MockRepo_Find_Returns(_mockUserRepo, existingAdmin);

            // 2. ACT
            var result = await _userService.RegisterAdmin(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.UserManagement.Register.USERNAME_EXIST);
            result.StatusCode.Should().Be(StatusCodeResponse.Conflict);

            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 3: ADMIN - INVALID EMAIL
        // ==========================================================
        [Fact]
        public async Task RegisterAdmin_WhenEmailFormatIsInvalid_ShouldReturnBadRequest()
        {
            // 1. ARRANGE
            var input = new RegisterAdminDTO
            {
                Username = "admin_test",
                Email = "invalid_email_format",
                Password = "AdminPass@123",
                FullName = "Admin Test",
                PhoneNumber = "0912345678"
            };

            // 2. ACT
            var result = await _userService.RegisterAdmin(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.UserManagement.Register.INVALID_EMAIL);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            Verify_Repo_Never_SingleOrDefaultAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 4: ADMIN - INVALID PHONE NUMBER
        // ==========================================================
        [Theory]
        [InlineData("123", MessageResponse.UserManagement.Register.INVALID_PHONE)]        // Quá ngắn
        [InlineData("012345678", MessageResponse.UserManagement.Register.INVALID_PHONE)]  // 9 chữ số
        [InlineData("01234567890", MessageResponse.UserManagement.Register.INVALID_PHONE)]// 11 chữ số
        [InlineData("091234567a", MessageResponse.UserManagement.Register.INVALID_PHONE)] // Có chữ
        public async Task RegisterAdmin_WhenPhoneNumberIsInvalid_ShouldReturnBadRequest(string invalidPhone, string expectedMsg)
        {
            // 1. ARRANGE
            var input = new RegisterAdminDTO
            {
                Username = "admin_test",
                Email = "admin@gmail.com",
                Password = "AdminPass@123",
                FullName = "Admin Test",
                PhoneNumber = invalidPhone
            };

            // 2. ACT
            var result = await _userService.RegisterAdmin(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(expectedMsg);
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            Verify_Repo_Never_SingleOrDefaultAsync<IUserRepository, User>(_mockUserRepo);
            Verify_Repo_Never_AddAsync<IUserRepository, User>(_mockUserRepo);
        }

        // ==========================================================
        // TEST CASE 5: ADMIN - DATABASE ERROR
        // ==========================================================
        [Fact]
        public async Task RegisterAdmin_WhenExceptionOccurs_ShouldReturnError()
        {
            // 1. ARRANGE
            var input = new RegisterAdminDTO
            {
                Username = "admin_test",
                Email = "admin@gmail.com",
                Password = "AdminPass@123",
                FullName = "Admin Test",
                PhoneNumber = "0912345678"
            };

            MockRepo_Find_Returns<IUserRepository, User>(_mockUserRepo, null);
            MockRepo_Add_ThrowsException<IUserRepository, User>(_mockUserRepo);

            // 2. ACT
            var result = await _userService.RegisterAdmin(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.UserManagement.Register.FAIL);
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
        }
        #endregion

        #region LoginUser Tests
        // ==========================================================
        // TEST CASE 1: ĐĂNG NHẬP THÀNH CÔNG (HAPPY PATH)
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

            // Giả lập DB trả về user có role
            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@gmail.com",
                PasswordHash = PasswordHelper.HashPassword("ValidPass@123"),
                FullName = "Test User",
                AvatarUrl = "https://example.com/avatar.jpg",
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
        // TEST CASE 2: ĐĂNG NHẬP - USER KHÔNG TỒN TẠI
        // ==========================================================
        [Fact]
        public async Task LoginUser_WhenUserNotFound_ShouldReturnNotFound()
        {
            // 1. ARRANGE
            var input = new LoginUserDTO
            {
                UsernameOrEmail = "notexist",
                Password = "ValidPass@123"
            };

            _mockUserRepo.Setup(x => x.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
            .ReturnsAsync((User?)null);

            // 2. ACT
            var result = await _userService.LoginUser(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        }

        // ==========================================================
        // TEST CASE 3: ĐĂNG NHẬP - PASSWORD SAI
        // ==========================================================
        [Fact]
        public async Task LoginUser_WhenPasswordIsIncorrect_ShouldReturnNotFound()
        {
            // 1. ARRANGE
            var input = new LoginUserDTO
            {
                UsernameOrEmail = "testuser",
                Password = "WrongPassword@123"
            };

            var user = new User
            {
                Id = 1,
                UserName = "testuser",
                Email = "test@gmail.com",
                PasswordHash = PasswordHelper.HashPassword("CorrectPass@123"),
                FullName = "Test User",
                UserRoles = new List<UserRole>()
            };

            _mockUserRepo.Setup(x => x.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
                .ReturnsAsync(user);

            // 2. ACT
            var result = await _userService.LoginUser(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.UserManagement.Login.INVALID_CREDENTIALS);
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
        }

        // ==========================================================
        // TEST CASE 4: ĐĂNG NHẬP - INVALID EMAIL FORMAT
        // ==========================================================
        [Fact]
        public async Task LoginUser_WhenEmailFormatIsInvalid_ShouldReturnBadRequest()
        {
            // 1. ARRANGE
            var input = new LoginUserDTO
            {
                UsernameOrEmail = "invalid_email",
                Password = "ValidPass@123"
            };

            // 2. ACT
            var result = await _userService.LoginUser(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            // Verify KHÔNG gọi GetUserWithRoles vì validation bị fail trước đó
            _mockUserRepo.Verify(x => x.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
        }

        // ==========================================================
        // TEST CASE 5: ĐĂNG NHẬP - EMPTY PASSWORD
        // ==========================================================
        [Fact]
        public async Task LoginUser_WhenPasswordIsEmpty_ShouldReturnBadRequest()
        {
            // 1. ARRANGE
            var input = new LoginUserDTO
            {
                UsernameOrEmail = "testuser",
                Password = ""
            };

            // 2. ACT
            var result = await _userService.LoginUser(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);

            // Verify KHÔNG gọi GetUserWithRoles
            _mockUserRepo.Verify(x => x.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
        }

        // ==========================================================
        // TEST CASE 6: ĐĂNG NHẬP - DATABASE ERROR
        // ==========================================================
        [Fact]
        public async Task LoginUser_WhenExceptionOccurs_ShouldReturnError()
        {
            // 1. ARRANGE
            var input = new LoginUserDTO
            {
                UsernameOrEmail = "testuser",
                Password = "ValidPass@123"
            };

            _mockUserRepo.Setup(x => x.GetUserWithRoles(It.IsAny<Expression<Func<User, bool>>>()))
                .ThrowsAsync(new Exception("Database Connection Error"));

            // 2. ACT
            var result = await _userService.LoginUser(input);

            // 3. ASSERT
            result.Content.Should().BeNull();
            result.Message.Should().Be(MessageResponse.UserManagement.Login.ERROR_IN_SERVER);
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
        }
    }
}
#endregion
