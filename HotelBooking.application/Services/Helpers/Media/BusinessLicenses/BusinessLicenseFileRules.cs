namespace HotelBooking.application.Services.Helpers.Media.BusinessLicenses;

// FR-ONB-011 / FR-MED-003: metadata and magic bytes must agree before any provider upload.
public static class BusinessLicenseFileRules
{
    public const int MaximumSize = 10 * 1024 * 1024;

    public static string? ExpectedMime(string? name) => Path.GetExtension(name)?.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => null,
    };

    public static bool MatchesSignature(string name, ReadOnlySpan<byte> bytes) => ExpectedMime(name) switch
    {
        "application/pdf" => bytes.StartsWith("%PDF-"u8),
        "image/jpeg" => bytes.StartsWith(new byte[] { 255, 216, 255 }),
        "image/png" => bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        _ => false,
    };

    public static Task<MemoryStream?> ReadContentAsync(Stream source, CancellationToken cancellationToken) =>
        HotelBooking.application.Services.Helpers.Media.BoundedMediaContent.ReadAsync(source, MaximumSize, cancellationToken);
}
