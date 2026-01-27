using Xunit;
using Moq;
using System.Threading.Tasks;

// 1. Using DTO và Service từ tầng Application
using HotelBooking.application.Services; // Nơi chứa UserService

// 2. Using Entity và Interface Repo từ tầng Infrastructure
using HotelBooking.infrastructure.Models;
using System.Linq.Expressions; // Nơi chứa class User, Room...

namespace HotelBooking.Tests.Services
{
    public class UserServiceTest
    {
        // 1. Khai báo các Mock cần thiết
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;

        // Khai báo thêm các Mock còn thiếu theo đúng Constructor của bạn
        private readonly Mock<IUserRoleRepository> _mockUserRoleRepo;
        private readonly Mock<IUpgradeRequestRepository> _mockUpgradeRepo;

        // Service cần test
        private readonly UserService _userService;

        public UserServiceTest()
        {
            // 2. Khởi tạo các Mock
            _mockUserRepo = new Mock<IUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
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

            // Giả lập DB trả về null (nghĩa là chưa có ai trùng Username hay Email cả)
            _mockUserRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync((User?)null);

            // Giả lập SaveChangesAsync trả về 1 (lưu thành công)
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            // Bước 1: Khẳng định nó không được null
            Assert.NotNull(result.Content);

            // Bước 2: Sau đó mới kiểm tra tiếp -> Hết Warning
            Assert.True(result.Content.IsSuccess);
            Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
            Assert.Equal(MessageRegister.REGISTER_SUCCESS, result.Message);

            // Quan trọng: Kiểm tra xem hàm AddAsync có được gọi đúng 1 lần không?
            _mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);

            // Kiểm tra SaveChangesAsync có được gọi 2 lần không? 
            // (Trong code của bạn gọi 2 lần: 1 lần lưu User, 1 lần lưu Role)
            _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
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

            // Giả lập DB tìm thấy 1 thằng trùng Username
            var existingUser = new User { UserName = "trung_ten", Email = "old@gmail.com" };

            _mockUserRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(existingUser);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            Assert.Null(result.Content);
            Assert.Equal(StatusCodeResponse.Conflict, result.StatusCode);
            Assert.Equal(MessageRegister.USERNAME_EXIST, result.Message); // Check đúng thông báo trùng user

            // Đảm bảo KHÔNG bao giờ gọi hàm lưu
            _mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
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

            // Giả lập DB tìm thấy 1 thằng trùng Email
            var existingUser = new User { UserName = "old_user", Email = "trung_email@gmail.com" };

            _mockUserRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(existingUser);

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            Assert.Null(result.Content);
            Assert.Equal(StatusCodeResponse.Conflict, result.StatusCode);
            Assert.Equal(MessageRegister.EMAIL_EXIST, result.Message); // Check đúng thông báo trùng email

            _mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
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

            // Giả lập chưa trùng ai cả
            _mockUserRepo.Setup(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync((User?)null);

            // NHƯNG khi gọi hàm AddAsync thì DB bị sập (Throw Exception)
            _mockUserRepo.Setup(x => x.AddAsync(It.IsAny<User>()))
                     .ThrowsAsync(new Exception("Database connection failed"));

            // 2. ACT
            var result = await _userService.RegisterCustomer(input);

            // 3. ASSERT
            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            Assert.Null(result.Content);
            Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
            Assert.Equal(MessageResponse.ERROR_IN_SERVER, result.Message);
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
            Assert.Null(result.Content);

            // Kiểm tra xem message có báo lỗi đúng ko (ví dụ: "Email không hợp lệ")
            Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
            // Lưu ý: Bạn cần có constant MessageRegister.EMAIL_INVALID hoặc check string cứng
            Assert.Equal(MessageRegister.INVALID_EMAIL, result.Message);

            // QUAN TRỌNG NHẤT:
            // Vì email sai ngay từ vòng gửi xe, nên KHÔNG ĐƯỢC PHÉP gọi xuống Database
            _mockUserRepo.Verify(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
            _mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
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
            // Kỳ vọng: Hệ thống phải trả về False và báo lỗi Bad Request (hoặc Error tùy quy định)
            Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);

            // QUAN TRỌNG: Vì ResponseFactory trả về default!, nên Content phải là NULL
            Assert.Null(result.Content);

            // QUAN TRỌNG: Giờ ta check chính xác câu thông báo lỗi
            Assert.Equal(expectedMsg, result.Message);

            // QUAN TRỌNG NHẤT:
            // Vì password rỗng ngay từ vòng gửi xe, nên KHÔNG ĐƯỢC PHÉP gọi xuống Database
            _mockUserRepo.Verify(x => x.SingleOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Never);
            _mockUserRepo.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        }
    }
}