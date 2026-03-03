using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration;

/// <summary>
/// DbContext dùng RIÊNG cho Integration Test.
/// 
/// VẤN ĐỀ: Vì project dùng DB-First, file HotelBookingDBContext.cs được scaffold tự động.
/// Trong đó OnConfiguring() luôn gọi UseSqlServer("Name=connectionStringHotelBooking")
/// → Khi ta truyền connection string khác (từ Docker container) qua constructor, nó bị conflict.
/// 
/// GIẢI PHÁP: Tạo class con, override OnConfiguring() để BỎ QUA cái connection string cứng.
/// Như vậy ta KHÔNG CẦN SỬA file generated (production code vẫn nguyên).
/// 
/// Schema (OnModelCreating) vẫn được kế thừa 100% từ cha → Tables tạo ra giống hệt production.
/// </summary>
public class TestHotelBookingDBContext : HotelBookingDBContext
{
    public TestHotelBookingDBContext(DbContextOptions<HotelBookingDBContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Override để BỎ QUA connection string cứng trong file scaffold.
    /// Connection thật sẽ được lấy từ DbContextOptions (truyền qua constructor).
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // KHÔNG gọi base.OnConfiguring() → bỏ qua UseSqlServer("Name=...")
        // Connection string từ Testcontainers đã được set qua DbContextOptions rồi
    }
}
