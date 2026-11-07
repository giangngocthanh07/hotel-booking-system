using System.Text.Json;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.RequestManagement;

[Trait("Requirement", "FR-ONB-016")]
[Trait("Requirement", "FR-CAT-006")]
[Trait("Requirement", "FR-CAT-007")]
[Trait("Category", "SqlIntegration")]
public class HotelPublicDetailsSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Theory]
    [InlineData(true, "Active", false, true)]
    [InlineData(true, "Active", null, true)]
    [InlineData(false, "Active", false, false)]
    [InlineData(null, "Active", false, false)]
    [InlineData(true, "Suspended", false, false)]
    [InlineData(true, "Closed", false, false)]
    [InlineData(true, "PendingVerification", false, false)]
    [InlineData(true, "Inactive", false, false)]
    [InlineData(true, null, false, false)]
    [InlineData(true, "", false, false)]
    [InlineData(true, "Active", true, false)]
    public async Task PublicDetails_RequiresVerifiedActiveAndNotDeleted(
        bool? verified, string? status, bool? deleted, bool visible)
    {
        var id = await SeedHotelAsync(verified, status, deleted);
        await using var context = fixture.CreateContext();
        var repository = CreateRepository(context);
        // Management CRUD remains separate: visibility does not become a global query filter.
        var stored = await repository.GetByIdAsync(id);
        Assert.NotNull(stored);
        Assert.Equal(verified, stored.IsVerified);
        Assert.Equal(status, stored.Status);
        Assert.Equal(deleted, stored.IsDeleted);
        var details = await repository.GetHotelDetailsByIdAsync(id);
        if (visible)
        {
            Assert.NotNull(details);
            Assert.Equal(id, details.Id);
            Assert.Equal(string.Empty, details.CoverImageUrl);
        }
        else Assert.Null(details);
    }

    [Fact]
    public async Task PublicDetails_MissingHotel_ReturnsNull()
    {
        await using var context = fixture.CreateContext();
        Assert.Null(await CreateRepository(context).GetHotelDetailsByIdAsync(-1));
    }

    private async Task<int> SeedHotelAsync(bool? verified, string? status, bool? deleted)
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        var additional = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!)!;
        var hotel = new Hotel
        {
            Name = seed.Name, Address = request.Address, OwnerId = seed.OwnerId, CoverImageUrl = string.Empty,
            CountryId = additional.Country.Id, ProvinceId = additional.Province.Id, WardId = additional.Ward.Id,
            PropertyTypeId = additional.PropType.Id, IsVerified = true, Status = "Active", IsDeleted = false,
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        // Update after insert so SQL defaults cannot replace the null legacy values under test.
        hotel.IsVerified = verified;
        hotel.Status = status;
        hotel.IsDeleted = deleted;
        await context.SaveChangesAsync();
        return hotel.Id;
    }

    private static HotelRepository CreateRepository(HotelBookingDBContext context) =>
        new(context, new HttpCancellationTokenProvider(new HttpContextAccessor()));
}
