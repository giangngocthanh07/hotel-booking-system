using System.Text.Json;
using System.Text.Json.Nodes;
using HotelBooking.application.DTOs.Hotel.Owner;
using HotelBooking.application.DTOs.Request.HotelApproval;
using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using HotelBooking.application.Validators.HotelManagement.Owner;
using HotelBooking.infrastructure.Models;
using HotelBooking.infrastructure.Shared;
using HotelBooking.Tests.Integration.RequestManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Tests.Integration.HotelManagement.Owner;

[Trait("Requirement", "FR-CAT-002")]
[Trait("Requirement", "FR-CAT-003")]
[Trait("Category", "SqlIntegration")]
public class OwnerHotelUpdateSqlTests(HotelRequestSqlFixture fixture) : IClassFixture<HotelRequestSqlFixture>
{
    [Fact]
    public async Task Update_PersistsInformationAndPreservesLegalOperationalAndFutureFields()
    {
        var (seed, id, input) = await SeedAsync();
        await using var context = fixture.CreateContext();
        await using var unit = new UnitOfWork(context);
        var service = Service(context, unit);
        Assert.Equal(StatusCodeResponse.Success, (await service.UpdateAsync(seed.OwnerId, id, input)).StatusCode);
        Assert.Equal(StatusCodeResponse.Success, (await service.UpdateAsync(seed.OwnerId, id, input)).StatusCode);
        await using var verify = fixture.CreateContext();
        var hotel = await verify.Hotels.SingleAsync(value => value.Id == id);
        Assert.Equal("Updated Hotel", hotel.Name);
        Assert.Equal(("Inactive", true, "cover.jpg", seed.OwnerId), (hotel.Status, hotel.IsVerified, hotel.CoverImageUrl, hotel.OwnerId));
        var metadata = JsonNode.Parse(hotel.Additional!)!;
        Assert.Equal("protected-tax", metadata["TaxCode"]!.GetValue<string>());
        Assert.Equal("private-reference", metadata["BusinessLicenseUrl"]!.GetValue<string>());
        Assert.Equal(9, metadata["Future"]!["Value"]!.GetValue<int>());
        Assert.Equal(0, metadata["Latitude"]!.GetValue<double>());
        Assert.Equal("hotel@example.com", metadata["PublicEmail"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("deleted")]
    [InlineData("unknown")]
    public async Task UnavailableHotel_DoesNotPersistEdits(string scenario)
    {
        var (seed, id, input) = await SeedAsync();
        await using var context = fixture.CreateContext();
        if (scenario == "deleted") await context.Hotels.Where(hotel => hotel.Id == id).ExecuteUpdateAsync(set => set.SetProperty(hotel => hotel.IsDeleted, true));
        await using var unit = new UnitOfWork(context);
        var ownerId = scenario == "foreign" ? seed.OtherAdminId : seed.OwnerId;
        var result = await Service(context, unit).UpdateAsync(ownerId, scenario == "unknown" ? int.MaxValue : id, input);
        Assert.Equal(StatusCodeResponse.NotFound, result.StatusCode);
        await using var verify = fixture.CreateContext();
        Assert.Equal("Original Hotel", (await verify.Hotels.SingleAsync(hotel => hotel.Id == id)).Name);
    }

    [Theory]
    [InlineData("country")]
    [InlineData("province")]
    [InlineData("ward")]
    public async Task MismatchedLocation_DoesNotPersistEdits(string field)
    {
        var (seed, id, input) = await SeedAsync();
        var other = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == other.RequestId);
        var location = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!)!;
        if (field == "country") input.CountryId = location.Country.Id;
        if (field == "province") input.ProvinceId = location.Province.Id;
        if (field == "ward") input.WardId = location.Ward.Id;
        await using var unit = new UnitOfWork(context);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service(context, unit).UpdateAsync(seed.OwnerId, id, input)).StatusCode);
        await using var verify = fixture.CreateContext();
        Assert.Equal("Original Hotel", (await verify.Hotels.SingleAsync(hotel => hotel.Id == id)).Name);
    }

    [Fact]
    public async Task TrackedUpdate_DoesNotOverwriteConcurrentStatusOrCoverChange()
    {
        var (seed, id, input) = await SeedAsync();
        await using var context = fixture.CreateContext();
        var repository = Repository(context);
        await repository.GetOwnedForUpdateAsync(seed.OwnerId, id);
        await using (var admin = fixture.CreateContext())
            await admin.Hotels.Where(hotel => hotel.Id == id).ExecuteUpdateAsync(set =>
                set.SetProperty(hotel => hotel.Status, "Suspended").SetProperty(hotel => hotel.CoverImageUrl, "new-cover.jpg"));
        await using var unit = new UnitOfWork(context);
        var service = new OwnerHotelUpdateService(repository, unit, new OwnerHotelUpdateValidator());
        Assert.Equal(StatusCodeResponse.Success, (await service.UpdateAsync(seed.OwnerId, id, input)).StatusCode);
        await using var verify = fixture.CreateContext();
        var stored = await verify.Hotels.SingleAsync(hotel => hotel.Id == id);
        Assert.Equal(("Suspended", "new-cover.jpg", "Updated Hotel"), (stored.Status, stored.CoverImageUrl, stored.Name));
    }

    private async Task<(HotelRequestSeed Seed, int Id, OwnerHotelUpdateDTO Input)> SeedAsync()
    {
        var seed = await fixture.SeedAsync();
        await using var context = fixture.CreateContext();
        var request = await context.HotelApprovalRequests.SingleAsync(value => value.Id == seed.RequestId);
        var info = JsonSerializer.Deserialize<HotelAdditionalInfo>(request.Additional!)!;
        var hotel = new Hotel
        {
            OwnerId = seed.OwnerId, Name = "Original Hotel", Address = "123 Original Street", Status = "Inactive", IsVerified = true,
            IsDeleted = false, CoverImageUrl = "cover.jpg", CountryId = info.Country.Id, ProvinceId = info.Province.Id,
            WardId = info.Ward.Id, PropertyTypeId = info.PropType.Id, CreatedAt = DateTime.UtcNow,
            Additional = "{\"TaxCode\":\"protected-tax\",\"BusinessLicenseUrl\":\"private-reference\",\"Future\":{\"Value\":9}}",
        };
        context.Hotels.Add(hotel);
        await context.SaveChangesAsync();
        return (seed, hotel.Id, new OwnerHotelUpdateDTO
        {
            Name = "Updated Hotel", Address = "123 Updated Street", Description = "Updated description", PublicPhone = "0901234567",
            PublicEmail = "hotel@example.com", CountryId = hotel.CountryId, ProvinceId = hotel.ProvinceId, WardId = hotel.WardId, Latitude = 0, Longitude = 106,
        });
    }

    private static HotelRepository Repository(HotelBookingDBContext context) =>
        new(context, new HttpCancellationTokenProvider(new HttpContextAccessor()));

    private static OwnerHotelUpdateService Service(HotelBookingDBContext context, IUnitOfWork unit) =>
        new(Repository(context), unit, new OwnerHotelUpdateValidator());
}
