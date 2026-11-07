using HotelBooking.infrastructure.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.RequestManagement;

[Trait("Requirement", "FR-ONB-016")]
[Trait("Requirement", "FR-CAT-006")]
[Trait("Requirement", "FR-CAT-007")]
[Trait("Category", "SqlIntegration")]
public class HotelPublicSearchSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Theory]
    [InlineData(true, "Active", false, true)]
    [InlineData(true, "Active", null, true)]
    [InlineData(false, "Active", false, false)]
    [InlineData(null, "Active", false, false)]
    [InlineData(true, "Suspended", false, false)]
    [InlineData(true, "Closed", false, false)]
    [InlineData(true, "PendingVerification", false, false)]
    [InlineData(true, null, false, false)]
    [InlineData(true, "Active", true, false)]
    public async Task Search_UsesCurrentProvinceSchemaAndPublicVisibility(bool? verified, string? status, bool? deleted, bool visible)
    {
        await InstallProcedureAsync();
        var seed = await HotelSearchSeed.CreateAsync(fixture);
        await using var context = fixture.CreateContext();
        var hotel = await context.Hotels.SingleAsync(value => value.Id == seed.HotelId);
        hotel.IsVerified = verified;
        hotel.Status = status;
        hotel.IsDeleted = deleted;
        await context.SaveChangesAsync();
        var results = await Repository(context).GetSearchHotelsAsync(seed.ProvinceName, null, null, null, null, null);
        if (!visible) { Assert.Empty(results); return; }
        var result = Assert.Single(results);
        Assert.Equal(seed.HotelId, result.Id);
        Assert.Equal(seed.ProvinceName, result.CityName);
        Assert.Equal(123.45m, result.PriceFrom);
        Assert.Equal(4.50m, result.AvgRating);
        Assert.Equal(2, result.ReviewCount);
        Assert.Equal(3, result.AvailableRooms);
    }

    [Theory]
    [InlineData(10, 12, 2)]
    [InlineData(9, 10, 3)]
    [InlineData(12, 13, 3)]
    public async Task Search_PreservesBookingOverlapAndRoomCount(int checkInDay, int checkOutDay, int available)
    {
        await InstallProcedureAsync();
        var seed = await HotelSearchSeed.CreateAsync(fixture);
        await using var context = fixture.CreateContext();
        await seed.ReserveOneRoomAsync(context);
        var repository = Repository(context);
        var checkIn = new DateTime(2035, 1, checkInDay);
        var checkOut = new DateTime(2035, 1, checkOutDay);
        var results = await repository.GetSearchHotelsAsync(seed.ProvinceName, checkIn, checkOut, 2, 1, available);
        Assert.Equal(available, Assert.Single(results).AvailableRooms);
        Assert.Empty(await repository.GetSearchHotelsAsync(seed.ProvinceName, checkIn, checkOut, 2, 1, available + 1));
        Assert.Empty(await repository.GetSearchHotelsAsync(seed.ProvinceName, checkIn, checkOut, 10, 10, 1));
        Assert.Empty(await repository.GetSearchHotelsAsync("No matching province", checkIn, checkOut, 2, 1, 1));
    }

    private async Task InstallProcedureAsync()
    {
        var sql = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Sql", "sp_SearchHotels.sql"));
        await using var context = fixture.CreateContext();
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    private static HotelRepository Repository(HotelBooking.infrastructure.Models.HotelBookingDBContext context) =>
        new(context, new HttpCancellationTokenProvider(new HttpContextAccessor()));
}
