using System.Text.Json;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using HotelBooking.Tests.Integration.RequestManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.Media.HotelImages;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-MED-005")]
[Trait("Requirement", "FR-MED-007")]
[Trait("Category", "SqlIntegration")]
public class OwnerHotelImageReadSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Theory]
    [InlineData("Inactive")]
    [InlineData("Suspended")]
    public async Task OwnedGallery_IncludesHiddenHotelsAndUsesStoredOrderWithIdTieBreaker(string status)
    {
        var (seed, id, expected) = await SeedAsync(status);
        await using var context = fixture.CreateContext();
        var result = await Service(context).GetAsync(seed.OwnerId, id);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("https://example.com/cover.jpg", result.Content!.CoverImageUrl);
        Assert.Equal(expected, result.Content.Gallery.Select(image => image.Id));
        Assert.Equal(new[] { 0, 0, 2 }, result.Content.Gallery.Select(image => image.DisplayOrder));
        Assert.Empty(context.ChangeTracker.Entries());
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("deleted")]
    [InlineData("unknown")]
    public async Task UnavailableHotel_ReturnsNoImages(string scenario)
    {
        var (seed, id, _) = await SeedAsync("Inactive");
        await using var context = fixture.CreateContext();
        if (scenario == "deleted") await context.Hotels.Where(hotel => hotel.Id == id).ExecuteUpdateAsync(set => set.SetProperty(hotel => hotel.IsDeleted, true));
        var result = await Service(context).GetAsync(scenario == "foreign" ? seed.OtherAdminId : seed.OwnerId,
            scenario == "unknown" ? int.MaxValue : id);
        Assert.Equal(StatusCodeResponse.NotFound, result.StatusCode);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task EmptyGallery_IsDifferentFromAnUnavailableHotel()
    {
        var (seed, id, _) = await SeedAsync("Inactive");
        await using var context = fixture.CreateContext();
        await context.HotelImages.Where(image => image.HotelId == id).ExecuteUpdateAsync(set => set.SetProperty(image => image.IsDeleted, true));
        var result = await Service(context).GetAsync(seed.OwnerId, id);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Empty(result.Content!.Gallery);
    }

    [Fact]
    public async Task PublicGallery_IsOrderedAndDoesNotReattachPretrackedDeletedImages()
    {
        var (_, id, expected) = await SeedAsync("Active");
        await using var context = fixture.CreateContext();
        Assert.Equal(4, (await context.HotelImages.Where(image => image.HotelId == id).ToListAsync()).Count);
        var repository = new HotelRepository(context, Token());
        var result = await repository.GetHotelDetailsByIdAsync(id);
        Assert.NotNull(result);
        Assert.Equal(expected, result.HotelImages.Select(image => image.Id));
        Assert.DoesNotContain(result.HotelImages, image => image.IsDeleted == true);
    }

    private async Task<(HotelRequestSeed Seed, int Id, int[] Expected)> SeedAsync(string status)
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        var info = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!)!;
        var hotel = new Hotel
        {
            Name = seed.Name, Address = request.Address, OwnerId = seed.OwnerId, IsDeleted = false, IsVerified = true, Status = status,
            CoverImageUrl = "https://example.com/cover.jpg", CountryId = info.Country.Id, ProvinceId = info.Province.Id,
            WardId = info.Ward.Id, PropertyTypeId = info.PropType.Id,
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        var images = new[] { Image(hotel.Id, 2, false), Image(hotel.Id, 0, false), Image(hotel.Id, 1, true), Image(hotel.Id, 0, false) };
        context.HotelImages.AddRange(images);
        await context.SaveChangesAsync();
        await context.HotelImages.Where(image => image.Id == images[3].Id).ExecuteUpdateAsync(set => set.SetProperty(image => image.IsDeleted, (bool?)null));
        return (seed, hotel.Id, new[] { images[1].Id, images[3].Id, images[0].Id });
    }

    private static HotelImage Image(int hotelId, int order, bool deleted) => new()
        { HotelId = hotelId, ImageUrl = $"https://example.com/{Guid.NewGuid():N}.jpg", DisplayOrder = order, IsDeleted = deleted };

    private static HttpCancellationTokenProvider Token() => new(new HttpContextAccessor());
    private static OwnerHotelImageReadService Service(HotelBookingDBContext context) => new(new HotelImageRepository(context, Token()));
}
