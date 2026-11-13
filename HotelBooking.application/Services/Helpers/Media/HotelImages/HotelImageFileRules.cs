namespace HotelBooking.application.Services.Helpers.Media.HotelImages;

public static class HotelImageFileRules
{
    public const int MaximumSize = 10 * 1024 * 1024;
    public static string? ExpectedMime(string? name) => Path.GetExtension(name)?.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => null,
    };

    public static bool MatchesSignature(string name, ReadOnlySpan<byte> bytes) => ExpectedMime(name) switch
    {
        "image/jpeg" => bytes.StartsWith(new byte[] { 255, 216, 255 }),
        "image/png" => bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
        _ => false,
    };
}
