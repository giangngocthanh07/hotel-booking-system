namespace HotelBooking.application.Services.Domains.Media;

// Internal provider metadata; never serialize asset identifiers as the public upload response.
public sealed record BusinessLicenseProviderUpload(string PublicId, string ResourceType, string DeliveryType, string StoredUrl)
{
    public bool IsValid() =>
        Fits(PublicId, 512) && Fits(ResourceType, 16) && Fits(DeliveryType, 32) && Fits(StoredUrl, 2048)
        && Uri.TryCreate(StoredUrl, UriKind.Absolute, out var url) && url.Scheme == Uri.UriSchemeHttps;

    private static bool Fits(string value, int length) => !string.IsNullOrWhiteSpace(value) && value.Length <= length;
}
