using HotelBooking.application.Validators.HotelManagement.Owner;
using HotelBooking.application.DTOs.Hotel.Owner;

namespace HotelBooking.test.UnitTests.Validators.HotelManagement.Owner;

[Trait("Requirement", "FR-CAT-002")]
public class OwnerHotelUpdateValidatorTests
{
    [Theory]
    [InlineData(null, null, true)]
    [InlineData(0.0, 0.0, true)]
    [InlineData(-90.0, -180.0, true)]
    [InlineData(90.0, 180.0, true)]
    [InlineData(0.0, null, false)]
    [InlineData(null, 0.0, false)]
    [InlineData(91.0, 106.0, false)]
    [InlineData(0.0, 181.0, false)]
    [InlineData(double.NaN, 106.0, false)]
    [InlineData(0.0, double.PositiveInfinity, false)]
    public async Task Coordinates_RequireFinitePairWithinRange(double? latitude, double? longitude, bool valid)
    {
        var input = new OwnerHotelUpdateDTO
        {
            Name = "Updated Hotel", Address = "123 Updated Street", PublicPhone = "0901234567", PublicEmail = "hotel@example.com",
            CountryId = 4, ProvinceId = 1, WardId = 2, Latitude = latitude, Longitude = longitude,
        };
        Assert.Equal(valid, (await new OwnerHotelUpdateValidator().ValidateAsync(input)).IsValid);
    }
}
