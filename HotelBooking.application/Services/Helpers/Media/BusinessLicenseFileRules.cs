namespace HotelBooking.application.Services.Helpers.Media;

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

    public static async Task<MemoryStream?> ReadContentAsync(Stream source, CancellationToken cancellationToken)
    {
        var content = new MemoryStream();
        try
        {
            if (source.CanSeek) source.Position = 0;
            var block = new byte[64 * 1024];
            while (content.Length <= MaximumSize)
            {
                // Read at most one byte past the limit, even when metadata lies or the stream cannot seek.
                var count = Math.Min(block.Length, MaximumSize + 1 - (int)content.Length);
                var read = await source.ReadAsync(block.AsMemory(0, count), cancellationToken);
                if (read == 0) { content.Position = 0; return content; }
                await content.WriteAsync(block.AsMemory(0, read), cancellationToken);
            }
            await content.DisposeAsync();
            return null;
        }
        catch { await content.DisposeAsync(); throw; }
    }
}
