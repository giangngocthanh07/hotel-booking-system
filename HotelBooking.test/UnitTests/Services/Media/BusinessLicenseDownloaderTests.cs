using System.Net;
using HotelBooking.application.Services.Domains.Media;
using Microsoft.Extensions.Configuration;

namespace HotelBooking.test.UnitTests.Services.Media;

[Trait("Requirement", "FR-ONB-013")]
public class BusinessLicenseDownloaderTests
{
    private static readonly byte[] Pdf = "%PDF-1.7"u8.ToArray();
    private static BusinessLicenseDownloadSource Source => new("owner/document.pdf", "raw", "private", "application/pdf", Pdf.Length);

    [Fact]
    public async Task Download_SignsPrivateRawRequestAndReturnsOriginalBytes()
    {
        var handler = new FakeHandler((request, _) =>
        {
            Assert.Equal("https", request.RequestUri!.Scheme);
            Assert.Equal("api.cloudinary.com", request.RequestUri.Host);
            Assert.Equal("/v1_1/test-cloud/raw/download", request.RequestUri.AbsolutePath);
            Assert.Contains("type=private", request.RequestUri.Query);
            Assert.Contains("signature=", request.RequestUri.Query);
            Assert.Contains("expires_at=", request.RequestUri.Query);
            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(request.RequestUri.Query);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Assert.InRange(long.Parse(query["expires_at"].ToString()), now + 45, now + 65);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Pdf) });
        });
        using var client = new HttpClient(handler);
        var result = await Service(client).DownloadAsync(Source);
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal(Pdf, result.Content);
    }

    [Theory]
    [InlineData("upload", 8)]
    [InlineData("private", 0)]
    [InlineData("private", 10485761)]
    public async Task InvalidRegistryMetadata_DoesNotContactProvider(string delivery, long size)
    {
        var handler = new FakeHandler((_, _) => throw new Xunit.Sdk.XunitException("Unexpected provider access."));
        using var client = new HttpClient(handler);
        var result = await Service(client).DownloadAsync(Source with { DeliveryType = delivery, SizeBytes = size });
        Assert.Equal(StatusCodeResponse.BadRequest, result.StatusCode);
        Assert.Null(result.Content);
    }

    [Theory]
    [InlineData(HttpStatusCode.Found)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ProviderFailure_DoesNotReturnBytes(HttpStatusCode status)
    {
        var handler = new FakeHandler((_, _) => Task.FromResult(new HttpResponseMessage(status)));
        using var client = new HttpClient(handler);
        var result = await Service(client).DownloadAsync(Source);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Null(result.Content);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(9)]
    [InlineData(10485761)]
    public async Task MismatchedOrOversizedContent_DoesNotReturnBytes(int size)
    {
        var handler = new FakeHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new ByteArrayContent(new byte[size]) }));
        using var client = new HttpClient(handler);
        var result = await Service(client).DownloadAsync(Source);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task Cancellation_PropagatesToHttpRequest()
    {
        using var cancellation = new CancellationTokenSource();
        var handler = new FakeHandler(async (_, token) =>
        {
            cancellation.Cancel();
            await Task.Delay(Timeout.Infinite, token);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var client = new HttpClient(handler);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Service(client).DownloadAsync(Source, cancellation.Token));
    }

    [Theory]
    [InlineData(10485760, true)]
    [InlineData(10485761, false)]
    [InlineData(10551296, false)]
    public async Task ChunkedResponse_IsBoundedAndDisposed(int length, bool succeeds)
    {
        var bytes = new byte[length];
        Pdf.CopyTo(bytes, 0);
        var stream = new TrackingStream(bytes);
        var handler = new FakeHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StreamContent(stream) }));
        using var client = new HttpClient(handler);
        var result = await Service(client).DownloadAsync(Source with { SizeBytes = 10485760 });
        Assert.Equal(succeeds ? StatusCodeResponse.Success : StatusCodeResponse.Error, result.StatusCode);
        Assert.True(stream.WasDisposed);
        Assert.Equal(Math.Min(length, 10485761), stream.BytesRead);
        if (succeeds) Assert.Equal(bytes, result.Content);
        else Assert.Null(result.Content);
    }

    [Fact]
    public async Task TransportError_DoesNotExposeSignedUrl()
    {
        var handler = new FakeHandler((_, _) => throw new HttpRequestException("https://provider/download?signature=secret"));
        using var client = new HttpClient(handler);
        var result = await Service(client).DownloadAsync(Source);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.DoesNotContain("signature", result.Message);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task SameSizeButInvalidSignature_DoesNotReturnBytes()
    {
        var handler = new FakeHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new ByteArrayContent(new byte[Pdf.Length]) }));
        using var client = new HttpClient(handler);
        var result = await Service(client).DownloadAsync(Source);
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Null(result.Content);
    }

    private sealed class TrackingStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public bool WasDisposed { get; private set; }
        public long BytesRead { get; private set; }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await base.ReadAsync(buffer, cancellationToken);
            BytesRead += read;
            return read;
        }

        protected override void Dispose(bool disposing)
        {
            WasDisposed = true;
            base.Dispose(disposing);
        }
    }

    private static BusinessLicenseDownloader Service(HttpClient client)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Cloudinary:CloudName"] = "test-cloud", ["Cloudinary:ApiKey"] = "test-key", ["Cloudinary:ApiSecret"] = "test-secret",
        }).Build();
        return new BusinessLicenseDownloader(client, configuration);
    }

    private sealed class FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            send(request, cancellationToken);
    }
}
