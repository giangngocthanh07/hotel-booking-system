using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration;

/// <summary>
/// DbContext specifically designed for Integration Testing.
/// 
/// THE CHALLENGE: Since the project uses a DB-First approach, the HotelBookingDBContext.cs file is automatically scaffolded.
/// By default, OnConfiguring() always calls UseSqlServer("Name=connectionStringHotelBooking").
/// This causes a conflict when we try to inject a different connection string (from the Docker container) via the constructor.
/// 
/// THE SOLUTION: Create this derived class and override OnConfiguring() to BYPASS the hardcoded connection string.
/// This way, we DON'T NEED TO MODIFY the generated file, keeping production code intact.
/// 
/// The schema (OnModelCreating) is 100% inherited from the parent -> generated tables will be identical to production.
/// </summary>
public class TestHotelBookingDBContext : HotelBookingDBContext
{
    public TestHotelBookingDBContext(DbContextOptions<HotelBookingDBContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Overrides to BYPASS the hardcoded connection string in the scaffolded file.
    /// The actual connection will be resolved from DbContextOptions passed via the constructor.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // DO NOT call base.OnConfiguring() -> this avoids executing UseSqlServer("Name=...")
        // The connection string from Testcontainers is already configured through DbContextOptions.
    }
}