using System.Text.Json;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using HotelBooking.Tests.Integration.RequestManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace HotelBooking.Tests.Integration.Media.HotelImages;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-CAT-006")]
[Trait("Requirement", "FR-MED-005")]
[Trait("Category", "SqlIntegration")]
public class OwnerHotelImageDeleteSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Theory]
    [InlineData("Inactive", 1, false, StatusCodeResponse.Success)]
    [InlineData("Active", 4, false, StatusCodeResponse.Conflict)]
    [InlineData("Active", 5, false, StatusCodeResponse.Success)]
    [InlineData("Active", 4, true, StatusCodeResponse.Success)]
    [InlineData("Active", 2, false, StatusCodeResponse.Conflict)]
    public async Task Removal_PreservesDistinctAdditionalImageRequirements(string status, int count, bool duplicate, string expected)
    {
        var seed = await SeedAsync(status, count, duplicate);
        var result = await RemoveAsync(seed.Owner, seed.Hotel, seed.Images[0]);
        Assert.Equal(expected, result.StatusCode);
        await using var verify = fixture.CreateContext();
        var rows = await verify.HotelImages.Where(image => image.HotelId == seed.Hotel).OrderBy(image => image.Id).ToListAsync();
        Assert.Equal(expected == StatusCodeResponse.Success, rows[0].IsDeleted);
        Assert.All(rows.Skip(1), image => Assert.False(image.IsDeleted));
        Assert.Equal(Enumerable.Range(0, rows.Count), rows.Select(image => image.DisplayOrder));
        Assert.Equal("https://example.com/cover.jpg", (await verify.Hotels.FindAsync(seed.Hotel))!.CoverImageUrl);
    }

    [Fact]
    public async Task ForeignOwnerOrImageCannotRemoveGallery()
    {
        var first = await SeedAsync("Inactive", 1);
        var other = await SeedAsync("Inactive", 1);
        Assert.Equal(StatusCodeResponse.NotFound, (await RemoveAsync(other.Owner, first.Hotel, first.Images[0])).StatusCode);
        Assert.Equal(StatusCodeResponse.NotFound, (await RemoveAsync(first.Owner, first.Hotel, other.Images[0])).StatusCode);
        await using var verify = fixture.CreateContext();
        Assert.False((await verify.HotelImages.FindAsync(first.Images[0]))!.IsDeleted);
        Assert.False((await verify.HotelImages.FindAsync(other.Images[0]))!.IsDeleted);
    }

    [Fact]
    public async Task DeletedImage_ReturnsNotFoundAndDisappearsFromOwnedReads()
    {
        var seed = await SeedAsync("Inactive", 1);
        Assert.Equal(StatusCodeResponse.Success, (await RemoveAsync(seed.Owner, seed.Hotel, seed.Images[0])).StatusCode);
        Assert.Equal(StatusCodeResponse.NotFound, (await RemoveAsync(seed.Owner, seed.Hotel, seed.Images[0])).StatusCode);
        await using var context = fixture.CreateContext();
        Assert.Empty((await Repository(context).GetOwnedGalleryAsync(seed.Owner, seed.Hotel))!.HotelImages);
    }

    [Fact]
    public async Task ConcurrentDeletes_FromFiveDistinctImagesOnlyOneCanSucceed()
    {
        var seed = await SeedAsync("Active", 5);
        var results = await Task.WhenAll(RemoveAsync(seed.Owner, seed.Hotel, seed.Images[0]), RemoveAsync(seed.Owner, seed.Hotel, seed.Images[1]));
        Assert.Single(results, result => result.StatusCode == StatusCodeResponse.Success);
        Assert.Single(results, result => result.StatusCode == StatusCodeResponse.Conflict);
        await using var verify = fixture.CreateContext();
        Assert.Equal(4, await verify.HotelImages.CountAsync(image => image.HotelId == seed.Hotel && image.IsDeleted != true));
    }

    [Fact]
    public async Task FailureAfterSqlWrite_RollsBackDeletion()
    {
        var seed = await SeedAsync("Inactive", 1);
        await using var context = new FailAfterSaveContext(fixture.Options);
        await using var unit = new UnitOfWork(context);
        var service = new OwnerHotelImageDeleteService(Repository(context), unit, NullLogger<OwnerHotelImageDeleteService>.Instance);
        Assert.Equal(StatusCodeResponse.Error, (await service.DeleteAsync(seed.Owner, seed.Hotel, seed.Images[0])).StatusCode);
        await using var verify = fixture.CreateContext();
        Assert.False((await verify.HotelImages.FindAsync(seed.Images[0]))!.IsDeleted);
    }

    private async Task<ApiResponse<bool>> RemoveAsync(int ownerId, int hotelId, int imageId)
    {
        await using var context = fixture.CreateContext();
        await using var unit = new UnitOfWork(context);
        return await new OwnerHotelImageDeleteService(Repository(context), unit, NullLogger<OwnerHotelImageDeleteService>.Instance)
            .DeleteAsync(ownerId, hotelId, imageId);
    }

    private async Task<(int Owner, int Hotel, int[] Images)> SeedAsync(string status, int count, bool duplicate = false)
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        var info = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!)!;
        var hotel = new Hotel { Name = seed.Name, OwnerId = seed.OwnerId, Address = request.Address, Status = status,
            IsVerified = true, IsDeleted = false, CoverImageUrl = "https://example.com/cover.jpg", CountryId = info.Country.Id,
            ProvinceId = info.Province.Id, WardId = info.Ward.Id, PropertyTypeId = info.PropType.Id };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        var images = Enumerable.Range(0, count + (duplicate ? 1 : 0)).Select(index => new HotelImage { HotelId = hotel.Id,
            ImageUrl = $"https://example.com/gallery-{index % count}.jpg", DisplayOrder = index, IsDeleted = false }).ToArray();
        context.HotelImages.AddRange(images);
        await context.SaveChangesAsync();
        return (seed.OwnerId, hotel.Id, images.Select(image => image.Id).ToArray());
    }

    private static HotelImageRepository Repository(HotelBookingDBContext context) =>
        new(context, new HttpCancellationTokenProvider(new HttpContextAccessor()));

    private sealed class FailAfterSaveContext(DbContextOptions<HotelBookingDBContext> options) : TestHotelBookingDBContext(options)
    {
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await base.SaveChangesAsync(cancellationToken);
            throw new IOException("Simulated failure after SQL write.");
        }
    }
}
