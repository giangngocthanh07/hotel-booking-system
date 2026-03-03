using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

// Dùng TestHotelBookingDBContext thay vì HotelBookingDBContext
// để tránh conflict với OnConfiguring hardcoded trong file DB-First scaffold
namespace HotelBooking.Tests.Integration;

/// <summary>
/// Base class cho Integration Test.
/// Tự động tạo SQL Server container bằng Docker, tạo DB, seed data cơ bản.
/// Mỗi Test Class kế thừa class này sẽ có 1 SQL Server container riêng.
/// 
/// KHÁC BIỆT VỚI UNIT TEST:
/// - Unit Test: Mock tất cả (Repository, UnitOfWork) → chỉ test logic
/// - Integration Test: Dùng DB thật → test cả logic + data layer + FK constraints
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    // ===== DOCKER CONTAINER =====
    // Testcontainers sẽ tự pull image SQL Server và tạo container
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")  // Image SQL Server
        .Build();

    protected IConfiguration _configuration = null!;
    protected HotelBookingDBContext _dbContext = null!;
    protected UnitOfWork _unitOfWork = null!;

    // Abstract → bắt buộc mỗi test class tự khai báo cần gì
    protected abstract IServiceProvider BuildServiceProvider(
        HotelBookingDBContext dbContext,
        IConfiguration config);

    protected IServiceProvider _serviceProvider = null!;

    /// <summary>
    /// xUnit gọi hàm này TRƯỚC KHI chạy bất kỳ test nào.
    /// Flow: Start Docker → Tạo DbContext → Tạo Tables → Seed Data
    /// </summary>
    public async Task InitializeAsync()
    {
        // 1. Load config từ appsettings.test.json (JWT key, Issuer, Audience...)
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.test.json")
            .Build();

        // 2. Khởi động SQL Server container (Docker tự pull image nếu chưa có)
        await _dbContainer.StartAsync();

        // 3. Tạo DbContext kết nối đến container
        //    ★ Dùng TestHotelBookingDBContext (class con) thay vì HotelBookingDBContext
        //    để tránh conflict với OnConfiguring() hardcoded trong file DB-First scaffold
        var options = new DbContextOptionsBuilder<HotelBookingDBContext>()
            .UseSqlServer(_dbContainer.GetConnectionString())  // Connection string tự sinh bởi Testcontainers
            .Options;

        _dbContext = new TestHotelBookingDBContext(options);

        // 4. Tạo tất cả tables dựa trên OnModelCreating (giống migration)
        await _dbContext.Database.EnsureCreatedAsync();

        // 5. Seed data cơ bản (Roles bắt buộc phải có)
        await SeedRolesAsync();

        // 6. Tạo UnitOfWork chung (không mock!)
        _unitOfWork = new UnitOfWork(_dbContext);

        // Gọi abstract method sau khi DB đã sẵn sàng
        _serviceProvider = BuildServiceProvider(_dbContext, _configuration);
    }

    /// <summary>
    /// Seed 3 Roles cơ bản vào DB.
    /// Bắt buộc vì RegisterCustomer/RegisterAdmin cần gán RoleId.
    /// 
    /// ★ VẤN ĐỀ DB-FIRST: Cột Id là IDENTITY (auto-increment) nên không thể insert Id bằng EF Core thông thường.
    /// GIẢI PHÁP: Dùng raw SQL với SET IDENTITY_INSERT ON để force đúng Id.
    /// </summary>
    private async Task SeedRolesAsync()
    {
        if (!await _dbContext.Roles.AnyAsync())
        {
            // Dùng raw SQL vì EF Core không cho insert explicit value vào identity column
            await _dbContext.Database.ExecuteSqlRawAsync(@"
                SET IDENTITY_INSERT Roles ON;
                INSERT INTO Roles (Id, Name, IsDeleted) VALUES (1, 'Admin', 0);
                INSERT INTO Roles (Id, Name, IsDeleted) VALUES (2, 'Customer', 0);
                INSERT INTO Roles (Id, Name, IsDeleted) VALUES (3, 'Owner', 0);
                SET IDENTITY_INSERT Roles OFF;
            ");
        }
    }

    /// <summary>
    /// Helper: Xóa sạch data sau mỗi test (giữ lại Roles).
    /// Gọi hàm này ở đầu mỗi test method để đảm bảo isolated.
    /// </summary>
    protected async Task CleanupDataAsync()
    {
        // Xóa theo thứ tự: con trước, cha sau (tránh FK constraint)
        _dbContext.UserRoles.RemoveRange(_dbContext.UserRoles);
        _dbContext.Users.RemoveRange(_dbContext.Users);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// xUnit gọi hàm này SAU KHI tất cả test chạy xong.
    /// Dọn dẹp: Dispose DbContext → Stop Docker container.
    /// </summary>
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}
