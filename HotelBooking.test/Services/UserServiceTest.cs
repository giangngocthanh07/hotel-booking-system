using Xunit;
using Moq;
using System.Threading.Tasks;
using FluentAssertions;

// 1. Using DTO và Service từ tầng Application
using HotelBooking.application.Services.Domains.UserManagement; // Nơi chứa UserService

// 2. Using Entity và Interface Repo từ tầng Infrastructure
using HotelBooking.infrastructure.Models;
using System.Linq.Expressions; // Nơi chứa class User, Room...

namespace HotelBooking.Tests.Services
{
    public class UserServiceTest : BaseServiceTest
    {
        // 1. Khai báo các Mock cần thiết
        private readonly Mock<IUserRepository> _mockUserRepo;

        // Khai báo thêm các Mock còn thiếu theo đúng Constructor của bạn
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<IUpgradeRequestRepository> _mockUpgradeRepo;

        // Service cần test
        private readonly UserService _userService;

        public UserServiceTest()
        {
            // 2. Khởi tạo các Mock
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUserRoleRepo = new Mock<IUserRoleRepository>();
            _mockUpgradeRepo = new Mock<IUpgradeRequestRepository>();

            // 3. Khởi tạo Service với đầy đủ 6 tham số
            // Thứ tự phải CHÍNH XÁC như trong file UserService.cs
            _userService = new UserService(
                null!,                       // 1. DBContext (Unit Test ko cần DB thật -> null)
                _mockUserRepo.Object,       // 2. UserRepository (Cái chính ta dùng)
                _mockUserRoleRepo.Object,   // 3. UserRoleRepository (Mock thêm cho đủ tụ)
                _mockUpgradeRepo.Object,    // 4. UpgradeRequestRepository (Mock thêm cho đủ tụ)
                null!,                       // 5. JwtAuthService (Hàm Register ko dùng Token -> null)
                _mockUnitOfWork.Object      // 6. UnitOfWork
            );
        }

        #region RegisterCustomer Tests
        // ==========================================================
        // TEST CASE 1: ĐĂNG KÝ THÀNH CÔNG (HAPPY PATH)
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
        [InlineData("", MessageRegister.EMPTY_PASSWORD)]          // Rỗng tuyệt đối
        [InlineData(null, MessageRegister.EMPTY_PASSWORD)]        // Null
        [InlineData("   ", MessageRegister.EMPTY_PASSWORD)]       // Chỉ có khoảng trắng (Space)

        // --- Nhóm Sai Định Dạng (Cái cũ) ---
        [InlineData("short", MessageRegister.SHORT_PASSWORD)]           // Quá ngắn (< 8 ký tự)
        [InlineData("nocapital1@", MessageRegister.UPPERCASE_LETTER_PASSWORD)]     // Thiếu chữ hoa
        [InlineData("NO_LOWER_123", MessageRegister.LOWERCASE_LETTER_PASSWORD)]    // Thiếu chữ thường
        [InlineData("NoSpecialChar1", MessageRegister.SPECIAL_CHARACTER_PASSWORD)]  // Thiếu ký tự đặc biệt
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
    }
    #endregion
}