namespace HotelBooking.application.Services.Helpers.Media;

public static class BoundedMediaContent
{
    public static async Task<MemoryStream?> ReadAsync(Stream source, int maximumSize, CancellationToken cancellationToken)
    {
        var content = new MemoryStream();
        try
        {
            if (source.CanSeek) source.Position = 0;
            var block = new byte[64 * 1024];
            while (content.Length <= maximumSize)
            {
                // FR-MED-003: bound actual reads even for non-seekable streams or incorrect declared lengths.
                var count = Math.Min(block.Length, maximumSize + 1 - (int)content.Length);
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
