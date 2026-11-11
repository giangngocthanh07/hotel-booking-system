using System.Text.Json;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using HotelBooking.application.Validators.Common;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using HotelBooking.Tests.Integration.RequestManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.HotelManagement.Owner;

[Trait("Requirement", "FR-CAT-001")]
[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-CAT-008")]
[Trait("Category", "SqlIntegration")]
public class OwnerHotelReadSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Fact]
    public async Task Pagination_CountsOnlyOwnedNonDeletedHotelsAndOrdersTiesById()
    {
        var (seed, ids) = await SeedHotelsAsync();
        await using var context = fixture.CreateContext();
        var service = Service(context);
        var first = await service.GetPageAsync(seed.OwnerId, new() { PageIndex = 1, PageSize = 2 });
        var second = await service.GetPageAsync(seed.OwnerId, new() { PageIndex = 2, PageSize = 2 });
        Assert.Equal(3, first.Content!.TotalCount);
        Assert.Equal(2, first.Content.TotalPages);
        Assert.Equal(new[] { ids[2], ids[1] }, first.Content.Items.Select(hotel => hotel.Id));
        Assert.Equal(ids[0], Assert.Single(second.Content!.Items).Id);
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task Details_ReturnsOwnedInactiveActiveAndSuspendedHotelsWithLocation()
    {
        var (seed, ids) = await SeedHotelsAsync();
        await using var context = fixture.CreateContext();
        var service = Service(context);
        var expected = new[] { "Inactive", "Active", "Suspended" };
        for (var index = 0; index < 3; index++)
        {
            var result = await service.GetByIdAsync(seed.OwnerId, ids[index]);
            Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
            Assert.Equal(expected[index], result.Content!.Status);
            Assert.NotEmpty(result.Content.CountryName);
            Assert.NotEmpty(result.Content.ProvinceName);
            Assert.NotEmpty(result.Content.WardName);
            Assert.NotEmpty(result.Content.PropertyTypeName);
        }
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Fact]
    public async Task ForeignDeletedAndUnknownHotels_ReturnSameNotFound()
    {
        var (seed, ids) = await SeedHotelsAsync();
        await using var context = fixture.CreateContext();
        var service = Service(context);
        foreach (var id in new[] { ids[3], ids[4], int.MaxValue })
        {
            var result = await service.GetByIdAsync(seed.OwnerId, id);
            Assert.Equal(StatusCodeResponse.NotFound, result.StatusCode);
            Assert.Equal("Hotel not found.", result.Message);
            Assert.Null(result.Content);
        }
        var empty = await service.GetPageAsync(int.MaxValue, new());
        Assert.Empty(empty.Content!.Items);
        Assert.Equal(0, empty.Content.TotalCount);
    }

    private async Task<(HotelRequestSeed Seed, int[] Ids)> SeedHotelsAsync()
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        var info = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!)!;
        var hotels = new[]
        {
            Hotel(seed.OwnerId, "Inactive", null, info), Hotel(seed.OwnerId, "Active", false, info),
            Hotel(seed.OwnerId, "Suspended", false, info), Hotel(seed.OtherAdminId, "Active", false, info),
            Hotel(seed.OwnerId, "Active", true, info),
        };
        context.Hotels.AddRange(hotels);
        await context.SaveChangesAsync();
        return (seed, hotels.Select(hotel => hotel.Id).ToArray());
    }

    private static Hotel Hotel(int ownerId, string status, bool? deleted, HotelAdditionalInfo info) => new()
    {
        OwnerId = ownerId, Name = $"Owner Hotel {Guid.NewGuid():N}", Address = "123 Owner Street",
        Status = status, IsDeleted = deleted, IsVerified = true, CreatedAt = new DateTime(2026, 1, 1),
        CountryId = info.Country.Id, ProvinceId = info.Province.Id, WardId = info.Ward.Id, PropertyTypeId = info.PropType.Id,
    };

    private static OwnerHotelService Service(HotelBookingDBContext context) => new(
        new HotelRepository(context, new HttpCancellationTokenProvider(new HttpContextAccessor())), new PagingRequestValidator());
}
