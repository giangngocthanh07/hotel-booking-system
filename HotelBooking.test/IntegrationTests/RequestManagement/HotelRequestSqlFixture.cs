using System.Text.Json;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace HotelBooking.Tests.Integration.RequestManagement;

public sealed record HotelRequestSeed(int RequestId, int OwnerId, int AdminId, int OtherAdminId, string Name);

public sealed class HotelRequestSqlFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();
    public DbContextOptions<HotelBookingDBContext> Options { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var connection = new SqlConnectionStringBuilder(_container.GetConnectionString())
            { InitialCatalog = "HotelRequestConcurrencyTests" };
        Options = new DbContextOptionsBuilder<HotelBookingDBContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var context = CreateContext();
        // Test-only mapped schema in a disposable container; no migrations or application DB changes.
        await context.Database.EnsureCreatedAsync();
    }

    public TestHotelBookingDBContext CreateContext() => new(Options);

    public async Task<HotelRequestSeed> SeedAsync()
    {
        await using var context = CreateContext();
        var suffix = Guid.NewGuid().ToString("N")[..12];
        var owner = CreateUser($"owner{suffix}");
        var admin = CreateUser($"admin{suffix}");
        var otherAdmin = CreateUser($"other{suffix}");
        var country = new Country { Name = $"Country{suffix}" };
        var type = new PropertyType { Name = $"Type{suffix}", IsActive = true };
        context.AddRange(owner, admin, otherAdmin, country, type);
        await context.SaveChangesAsync();
        var ward = await SeedLocationAsync(context, country.Id, suffix);
        var request = CreateRequest(owner.Id, country.Id, type.Id, ward, suffix);
        context.HotelApprovalRequests.Add(request);
        await context.SaveChangesAsync();
        return new(request.Id, owner.Id, admin.Id, otherAdmin.Id, request.Name);
    }

    private static async Task<Ward> SeedLocationAsync(HotelBookingDBContext context, int countryId, string suffix)
    {
        // DB-first Province codes are assigned values; Ward IDs remain identity columns.
        var province = new Province { Id = countryId, CountryId = countryId, Name = $"Province{suffix}", Type = "Province" };
        context.Provinces.Add(province);
        await context.SaveChangesAsync();
        var ward = new Ward { ProvinceId = province.Id, Name = $"Ward{suffix}", Type = "Ward" };
        context.Wards.Add(ward);
        await context.SaveChangesAsync();
        return ward;
    }

    private static HotelApprovalRequest CreateRequest(int ownerId, int countryId, int typeId, Ward ward, string suffix) => new()
    {
        OwnerId = ownerId, Name = $"Hotel{suffix}", Status = "Pending", Address = "123 Integration Street",
        TaxCode = "1234567890", BusinessLicenseUrl = "https://example.com/license.pdf", CreatedAt = DateTime.UtcNow,
        Additional = JsonSerializer.Serialize(new HotelAdditionalInfo
        {
            PropType = new PropertyTypeDTO { Id = typeId }, Country = new CountryDTO { Id = countryId },
            Province = new ProvinceDTO { Id = ward.ProvinceId }, Ward = new WardDTO { Id = ward.Id },
        }),
    };

    private static User CreateUser(string name) => new()
    {
        UserName = name, Email = $"{name}@example.com", PasswordHash = "integration-test-placeholder",
        PhoneNumber = "0901234567", IsActive = true, IsDeleted = false,
    };

    public async Task DisposeAsync() => await _container.DisposeAsync();
}
