using System.Text.Json.Nodes;
using HotelBooking.application.DTOs.Hotel.Owner;
using HotelBooking.application.Services.Domains.HotelManagement.Owner;
using HotelBooking.application.Validators.HotelManagement.Owner;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.HotelManagement.Owner;

[Trait("Requirement", "FR-CAT-002")]
[Trait("Requirement", "FR-CAT-003")]
public class OwnerHotelUpdateServiceTests
{
    private readonly Mock<IHotelRepository> _hotels = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unit = new(MockBehavior.Strict);
    private OwnerHotelUpdateService Service => new(_hotels.Object, _unit.Object, new OwnerHotelUpdateValidator());

    [Fact]
    public async Task Update_MergesEditableFieldsAndPreservesLegalAndUnknownMetadata()
    {
        var hotel = Hotel();
        Setup(hotel);
        using var cancellation = new CancellationTokenSource();
        var result = await Service.UpdateAsync(7, 42, Input(), cancellation.Token);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("Updated Hotel", hotel.Name);
        Assert.Equal(("Suspended", true, 7, "cover.jpg"), (hotel.Status, hotel.IsVerified, hotel.OwnerId, hotel.CoverImageUrl));
        var metadata = JsonNode.Parse(hotel.Additional!)!.AsObject();
        Assert.Equal("legal-tax", metadata["TaxCode"]!.GetValue<string>());
        Assert.Equal("private-reference", metadata["BusinessLicenseUrl"]!.GetValue<string>());
        Assert.Equal(9, metadata["Future"]!["Value"]!.GetValue<int>());
        Assert.Equal("0901234567", metadata["PublicPhone"]!.GetValue<string>());
        Assert.False(metadata.ContainsKey("publicPhone"));
        Assert.Equal(0, metadata["Latitude"]!.GetValue<double>());
        _unit.Verify(unit => unit.TrySaveChangesAsync(cancellation.Token), Times.Once);
    }

    [Fact]
    public async Task MissingOrForeignHotel_DoesNotValidateLocationOrSave()
    {
        _hotels.Setup(repo => repo.GetOwnedForUpdateAsync(7, 42, default)).ReturnsAsync((Hotel?)null);
        Assert.Equal(StatusCodeResponse.NotFound, (await Service.UpdateAsync(7, 42, Input())).StatusCode);
        _hotels.Verify(repo => repo.IsLocationValidAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidLocation_DoesNotMutateOrSave()
    {
        var hotel = Hotel();
        _hotels.Setup(repo => repo.GetOwnedForUpdateAsync(7, 42, default)).ReturnsAsync(hotel);
        _hotels.Setup(repo => repo.IsLocationValidAsync(4, 1, 2, default)).ReturnsAsync(false);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UpdateAsync(7, 42, Input())).StatusCode);
        Assert.Equal("Original Hotel", hotel.Name);
        _unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("broken json")]
    [InlineData("[]")]
    public async Task MalformedMetadata_IsNotOverwritten(string json)
    {
        var hotel = Hotel();
        hotel.Additional = json;
        Setup(hotel);
        Assert.Equal(StatusCodeResponse.Conflict, (await Service.UpdateAsync(7, 42, Input())).StatusCode);
        Assert.Equal(json, hotel.Additional);
        Assert.Equal("Original Hotel", hotel.Name);
        _unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0, false, StatusCodeResponse.Success)]
    [InlineData(1, false, StatusCodeResponse.Success)]
    [InlineData(0, true, StatusCodeResponse.Conflict)]
    public async Task SaveResult_HandlesNoOpAndConflicts(int rows, bool conflict, string status)
    {
        Setup(Hotel());
        _unit.Setup(unit => unit.TrySaveChangesAsync(default)).ReturnsAsync(new SaveChangesResult(rows, conflict));
        Assert.Equal(status, (await Service.UpdateAsync(7, 42, Input())).StatusCode);
        _unit.Verify(unit => unit.TrySaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task InvalidInput_DoesNotReadOrSave()
    {
        var input = Input();
        input.Name = "short";
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UpdateAsync(7, 42, input)).StatusCode);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UpdateAsync(7, 0, Input())).StatusCode);
        Assert.Equal(StatusCodeResponse.Unauthorized, (await Service.UpdateAsync(0, 42, Input())).StatusCode);
        _hotels.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SaveCancellation_Propagates()
    {
        Setup(Hotel());
        using var cancellation = new CancellationTokenSource();
        _unit.Setup(unit => unit.TrySaveChangesAsync(cancellation.Token)).ThrowsAsync(new OperationCanceledException());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service.UpdateAsync(7, 42, Input(), cancellation.Token));
    }

    [Fact]
    public async Task EmptyCoordinates_ClearOnlyCoordinates()
    {
        var hotel = Hotel();
        Setup(hotel);
        var input = Input();
        input.Latitude = input.Longitude = null;
        Assert.Equal(StatusCodeResponse.Success, (await Service.UpdateAsync(7, 42, input)).StatusCode);
        var metadata = JsonNode.Parse(hotel.Additional!)!;
        Assert.Null(metadata["Latitude"]);
        Assert.Null(metadata["Longitude"]);
        Assert.Equal("legal-tax", metadata["TaxCode"]!.GetValue<string>());
    }

    private void Setup(Hotel hotel)
    {
        _hotels.Setup(repo => repo.GetOwnedForUpdateAsync(7, 42, It.IsAny<CancellationToken>())).ReturnsAsync(hotel);
        _hotels.Setup(repo => repo.IsLocationValidAsync(4, 1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _unit.Setup(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new SaveChangesResult(1, false));
    }

    private static Hotel Hotel() => new()
    {
        Id = 42, OwnerId = 7, Name = "Original Hotel", Status = "Suspended", IsVerified = true, CoverImageUrl = "cover.jpg",
        Additional = "{\"TaxCode\":\"legal-tax\",\"BusinessLicenseUrl\":\"private-reference\",\"publicPhone\":\"old\",\"Future\":{\"Value\":9}}",
    };

    private static OwnerHotelUpdateDTO Input() => new()
    {
        Name = "Updated Hotel", Description = "New description", Address = "123 Updated Street", PublicPhone = "0901234567",
        PublicEmail = "hotel@example.com", CountryId = 4, ProvinceId = 1, WardId = 2, Latitude = 0, Longitude = 106,
    };
}
