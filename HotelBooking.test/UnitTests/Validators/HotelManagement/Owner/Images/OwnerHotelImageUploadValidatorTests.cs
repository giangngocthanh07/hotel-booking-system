using HotelBooking.application.DTOs.Hotel.Owner.Images;
using HotelBooking.application.Validators.HotelManagement.Owner.Images;

namespace HotelBooking.test.UnitTests.Validators.HotelManagement.Owner.Images;

[Trait("Requirement", "FR-MED-003")]
public class OwnerHotelImageUploadValidatorTests
{
    [Fact]
    public async Task MismatchedFormat_ExplainsAcceptedPhotosWithoutTechnicalJargon()
    {
        using var stream = new MemoryStream(new byte[] { 1 });
        var file = new OwnerHotelImageFileDTO { FileName = "photo.png", ContentType = "image/jpeg", Size = 1, Content = stream };
        var result = await new OwnerHotelImageUploadValidator().ValidateAsync(file);
        var message = Assert.Single(result.Errors).ErrorMessage;
        Assert.Contains("JPG, JPEG or PNG", message);
        Assert.DoesNotContain("MIME", message);
        Assert.Contains("Renaming", message);
    }
    [Theory]
    [InlineData("a.jpg", "image/jpeg", 3, true)]
    [InlineData("a.JPEG", "IMAGE/JPEG", 3, true)]
    [InlineData("a.png", "image/png", 10485760, true)]
    [InlineData("a.pdf", "application/pdf", 3, false)]
    [InlineData("a.gif", "image/gif", 3, false)]
    [InlineData("a.png", "image/jpeg", 3, false)]
    [InlineData("a.jpg", "image/jpeg", 0, false)]
    [InlineData("a.jpg", "image/jpeg", 10485761, false)]
    public async Task Metadata_RequiresSupportedImageAndBoundedSize(string name, string mime, long size, bool valid)
    {
        await using var stream = new MemoryStream(new byte[] { 255, 216, 255 });
        var file = new OwnerHotelImageFileDTO { FileName = name, ContentType = mime, Size = size, Content = stream };
        Assert.Equal(valid, (await new OwnerHotelImageUploadValidator().ValidateAsync(file)).IsValid);
    }
}
