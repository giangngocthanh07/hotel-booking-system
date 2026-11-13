using System.Text.Json;
using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.HotelManagement.Owner.Images;
using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Validators.HotelManagement.Owner.Images;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using HotelBooking.Tests.Integration.RequestManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HotelBooking.Tests.Integration.Media.HotelImages;

[Trait("Requirement", "FR-CAT-003")]
[Trait("Requirement", "FR-CAT-006")]
[Trait("Requirement", "FR-MED-001")]
[Trait("Requirement", "FR-MED-002")]
[Trait("Requirement", "FR-MED-005")]
[Trait("Category", "SqlIntegration")]
public class OwnerHotelImageUploadSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Upload_PersistsProviderUrlWithoutChangingProtectedFields(bool cover)
    {
        var (seed, id) = await SeedAsync();
        var result = await UploadAsync(seed.OwnerId, id, cover, Provider().Object);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        await using var verify = fixture.CreateContext();
        var hotel = await verify.Hotels.SingleAsync(value => value.Id == id);
        Assert.Equal(("Inactive", true, seed.OwnerId, "legal"), (hotel.Status, hotel.IsVerified, hotel.OwnerId, hotel.Additional));
        Assert.Equal(cover ? "https://example.com/new.jpg" : "https://example.com/old.jpg", hotel.CoverImageUrl);
        if (cover) Assert.Empty(await verify.HotelImages.Where(image => image.HotelId == id).ToListAsync());
        else
        {
            var image = await verify.HotelImages.SingleAsync(value => value.HotelId == id);
            Assert.Equal(image.Id, result.Content!.ImageId);
            Assert.Equal((0, false, "https://example.com/new.jpg"), (image.DisplayOrder, image.IsDeleted, image.ImageUrl));
        }
    }

    [Fact]
    public async Task ConcurrentGalleryUploads_GetDistinctConsecutivePositions()
    {
        var (seed, id) = await SeedAsync();
        var provider = ConcurrentProvider();
        var results = await Task.WhenAll(Enumerable.Range(0, 3).Select(_ => UploadAsync(seed.OwnerId, id, false, provider.Object)));
        Assert.All(results, result => Assert.Equal(StatusCodeResponse.Success, result.StatusCode));
        await using var verify = fixture.CreateContext();
        var rows = await verify.HotelImages.Where(image => image.HotelId == id).OrderBy(image => image.DisplayOrder).ToListAsync();
        Assert.Equal(new[] { 0, 1, 2 }, rows.Select(image => image.DisplayOrder));
        Assert.Equal(3, rows.Select(image => image.ImageUrl).Distinct().Count());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task FailureAfterSqlWrite_RollsBackCoverOrGallery(bool cover)
    {
        var (seed, id) = await SeedAsync();
        await using var context = new FailAfterSaveContext(fixture.Options);
        await using var unit = new UnitOfWork(context);
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        var result = await Service(context, unit, Provider().Object).UploadAsync(seed.OwnerId, id, File(content), cover);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Null(result.Content);
        await using var verify = fixture.CreateContext();
        Assert.Equal("https://example.com/old.jpg", (await verify.Hotels.SingleAsync(hotel => hotel.Id == id)).CoverImageUrl);
        Assert.False(await verify.HotelImages.AnyAsync(image => image.HotelId == id));
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("deleted")]
    public async Task OwnershipRecheck_AfterProviderUploadPreventsPersistence(string change)
    {
        var (seed, id) = await SeedAsync();
        var provider = new Mock<IPhotoService>(MockBehavior.Strict);
        provider.Setup(photo => photo.UploadHotelImageAsync(It.IsAny<UploadFileDTO>(), seed.OwnerId, id, true, default))
            .Returns(async () =>
            {
                await using var admin = fixture.CreateContext();
                if (change == "owner") await admin.Hotels.Where(hotel => hotel.Id == id).ExecuteUpdateAsync(set => set.SetProperty(hotel => hotel.OwnerId, seed.OtherAdminId));
                else await admin.Hotels.Where(hotel => hotel.Id == id).ExecuteUpdateAsync(set => set.SetProperty(hotel => hotel.IsDeleted, true));
                return ProviderResponse("https://example.com/new.jpg");
            });
        Assert.Equal(StatusCodeResponse.NotFound, (await UploadAsync(seed.OwnerId, id, true, provider.Object)).StatusCode);
        await using var verify = fixture.CreateContext();
        Assert.Equal("https://example.com/old.jpg", (await verify.Hotels.SingleAsync(hotel => hotel.Id == id)).CoverImageUrl);
    }

    [Theory]
    [InlineData(4, false, StatusCodeResponse.Conflict)]
    [InlineData(5, false, StatusCodeResponse.Success)]
    [InlineData(4, true, StatusCodeResponse.Conflict)]
    public async Task ActiveCover_UsesDistinctAdditionalImagesExcludingCover(int unique, bool duplicate, string status)
    {
        var (seed, id) = await SeedAsync();
        await using (var context = fixture.CreateContext())
        {
            await context.Hotels.Where(hotel => hotel.Id == id).ExecuteUpdateAsync(set => set.SetProperty(hotel => hotel.Status, "Active"));
            var urls = Enumerable.Range(0, unique).Select(index => index == 0 ? "https://example.com/new.jpg" : $"https://example.com/{index}.jpg").ToList();
            if (duplicate) urls.Add(urls[1]);
            context.HotelImages.AddRange(urls.Select((url, index) => new HotelImage { HotelId = id, ImageUrl = url, DisplayOrder = index, IsDeleted = false }));
            await context.SaveChangesAsync();
        }
        var result = await UploadAsync(seed.OwnerId, id, true, Provider().Object);
        Assert.Equal(status, result.StatusCode);
        await using var verify = fixture.CreateContext();
        Assert.Equal(status == StatusCodeResponse.Success ? "https://example.com/new.jpg" : "https://example.com/old.jpg",
            (await verify.Hotels.SingleAsync(hotel => hotel.Id == id)).CoverImageUrl);
    }

    private async Task<ApiResponse<OwnerHotelImageUploadDTO>> UploadAsync(int ownerId, int hotelId, bool cover, IPhotoService provider)
    {
        await using var context = fixture.CreateContext();
        await using var unit = new UnitOfWork(context);
        await using var content = new MemoryStream(new byte[] { 255, 216, 255 });
        return await Service(context, unit, provider).UploadAsync(ownerId, hotelId, File(content), cover);
    }

    private async Task<(HotelRequestSeed Seed, int Id)> SeedAsync()
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        var info = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!)!;
        var hotel = new Hotel
        {
            Name = seed.Name, OwnerId = seed.OwnerId, Address = request.Address, Status = "Inactive", IsVerified = true, IsDeleted = false,
            CoverImageUrl = "https://example.com/old.jpg", Additional = "legal", CountryId = info.Country.Id,
            ProvinceId = info.Province.Id, WardId = info.Ward.Id, PropertyTypeId = info.PropType.Id,
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return (seed, hotel.Id);
    }

    private static OwnerHotelImageUploadService Service(HotelBookingDBContext context, IUnitOfWork unit, IPhotoService provider) =>
        new(new HotelImageRepository(context, new HttpCancellationTokenProvider(new HttpContextAccessor())), provider, unit,
            new OwnerHotelImageUploadValidator(), NullLogger<OwnerHotelImageUploadService>.Instance);

    private static Mock<IPhotoService> Provider()
    {
        var provider = new Mock<IPhotoService>(MockBehavior.Strict);
        provider.Setup(photo => photo.UploadHotelImageAsync(It.IsAny<UploadFileDTO>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProviderResponse("https://example.com/new.jpg"));
        return provider;
    }

    private static Mock<IPhotoService> ConcurrentProvider()
    {
        var provider = new Mock<IPhotoService>(MockBehavior.Strict);
        var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var uploaded = 0;
        provider.Setup(photo => photo.UploadHotelImageAsync(It.IsAny<UploadFileDTO>(), It.IsAny<int>(), It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
            .Returns(async (UploadFileDTO _, int _, int _, bool _, CancellationToken token) =>
            {
                var index = Interlocked.Increment(ref uploaded);
                if (index == 3) ready.TrySetResult();
                await ready.Task.WaitAsync(TimeSpan.FromSeconds(20), token);
                return ProviderResponse($"https://example.com/{index}.jpg");
            });
        return provider;
    }

    private static ApiResponse<UploadResultDTO> ProviderResponse(string url) =>
        ResponseFactory.Success(new UploadResultDTO { Uploaded = true, StoredFileName = url }, "Uploaded.");
    private static OwnerHotelImageFileDTO File(Stream content) => new() { FileName = "hotel.jpg", ContentType = "image/jpeg", Size = 3, Content = content };

    private sealed class FailAfterSaveContext(DbContextOptions<HotelBookingDBContext> options) : TestHotelBookingDBContext(options)
    {
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await base.SaveChangesAsync(cancellationToken);
            throw new IOException("Simulated failure after the SQL write.");
        }
    }
}
