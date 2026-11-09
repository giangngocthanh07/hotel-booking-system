using System.Text.Json;
using HotelBooking.application.DTOs.Request.HotelApproval;

namespace HotelBooking.test.UnitTests.Services.Media;

[Trait("Requirement", "FR-ONB-013")]
public class BusinessLicenseSerializationTests
{
    [Fact]
    public void RegistrationResponse_DoesNotSerializeProviderUrl()
    {
        var json = JsonSerializer.Serialize(new HotelRegistrationDetailDTO { BusinessLicenseUrl = "https://provider/secret.pdf" });
        Assert.DoesNotContain("BusinessLicenseUrl", json);
        Assert.DoesNotContain("https://provider", json);
    }

    [Fact]
    public void HotelMetadata_DoesNotSerializeLicenseUrl()
    {
        var json = JsonSerializer.Serialize(new HotelEntityAdditionalData { BusinessLicenseUrl = "https://provider/secret.pdf" });
        Assert.DoesNotContain("BusinessLicenseUrl", json);
        Assert.DoesNotContain("https://provider", json);
    }
}
