using HotelBooking.application.Services.Domains.Media;
using HotelBooking.application.Validators.Media;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace HotelBooking.test.UnitTests.Services.Media;

[Trait("Requirement", "FR-ONB-011")]
[Trait("Requirement", "FR-MED-003")]
public class BusinessLicenseValidationTests
{
    private readonly Mock<IBusinessLicenseUploader> _uploader = new(MockBehavior.Strict);
    private readonly Mock<IBusinessLicenseUploadRepository> _uploads = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _unit = new(MockBehavior.Strict);
    private FileService Service => new(_uploader.Object, new BusinessLicenseUploadValidator(), NullLogger<FileService>.Instance, _uploads.Object, _unit.Object);

    public BusinessLicenseValidationTests()
    {
        _uploads.Setup(repo => repo.AddAsync(It.IsAny<HotelBooking.infrastructure.Models.BusinessLicenseUpload>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unit.Setup(unit => unit.TrySaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new SaveChangesResult(1, false));
    }
    private static readonly byte[] Pdf = "%PDF-1.7\nexample"u8.ToArray();
    private static readonly byte[] Jpeg = [255, 216, 255, 224, 0, 16];
    private static readonly byte[] Png = [137, 80, 78, 71, 13, 10, 26, 10, 0];

    [Theory]
    [InlineData("license.pdf", "application/pdf")]
    [InlineData("LICENSE.PDF", "APPLICATION/PDF")]
    [InlineData("license.jpg", "image/jpeg")]
    [InlineData("license.jpeg", "image/jpeg")]
    [InlineData("license.png", "image/png")]
    public async Task MatchingFormats_PreserveAllBytesAndCancellation(string name, string mime)
    {
        var bytes = name.EndsWith("pdf", StringComparison.OrdinalIgnoreCase) ? Pdf : mime == "image/png" ? Png : Jpeg;
        await using var input = new MemoryStream(bytes);
        input.Position = 1; // Seekable callers must still upload the complete file from its beginning.
        var file = File(name, mime, input, bytes.Length);
        using var cancellation = new CancellationTokenSource();
        Stream? providerContent = null;
        _uploader.Setup(provider => provider.UploadAsync(It.IsAny<UploadFileDTO>(), 7, cancellation.Token))
            .Returns(async (UploadFileDTO upload, int _, CancellationToken token) =>
            {
                providerContent = upload.Content;
                Assert.Equal(0, upload.Content.Position);
                using var copy = new MemoryStream();
                await upload.Content.CopyToAsync(copy, token);
                Assert.Equal(bytes, copy.ToArray());
                return Success();
            });
        Assert.Equal(StatusCodeResponse.Success, (await Service.UploadBusinessLicenseAsync(file, 7, cancellation.Token)).StatusCode);
        Assert.False(providerContent!.CanRead);
        Assert.True(input.CanRead);
        _uploader.VerifyAll();
    }

    [Theory]
    [InlineData("license.exe", "application/pdf")]
    [InlineData("license.pdf", "image/png")]
    [InlineData("license.jpg", "image/png")]
    [InlineData("license.png", "image/jpeg")]
    [InlineData("license.pdf", "application/octet-stream")]
    [InlineData("license.pdf", "")]
    [InlineData("", "application/pdf")]
    public async Task InvalidMetadata_DoesNotUpload(string name, string mime)
    {
        await using var content = new MemoryStream(Pdf);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadBusinessLicenseAsync(File(name, mime, content, Pdf.Length), 7)).StatusCode);
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("license.pdf", "application/pdf")]
    [InlineData("license.jpg", "image/jpeg")]
    [InlineData("license.png", "image/png")]
    public async Task SpoofedOrTruncatedSignature_DoesNotUpload(string name, string mime)
    {
        await using var content = new MemoryStream([0, 1, 2]);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadBusinessLicenseAsync(File(name, mime, content, 3), 7)).StatusCode);
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10485761)]
    public async Task InvalidDeclaredSize_DoesNotReadOrUpload(long size)
    {
        var file = File("license.pdf", "application/pdf", Stream.Null, size);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadBusinessLicenseAsync(file, 7)).StatusCode);
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(10485760, 10485760, true)]
    [InlineData(10485761, 10485760, false)]
    [InlineData(20, 10, false)]
    [InlineData(0, 10, false)]
    public async Task ActualSize_IsBoundedAndMustMatchMetadata(int actualSize, long declaredSize, bool valid)
    {
        var bytes = new byte[actualSize];
        if (actualSize >= Pdf.Length) Pdf.CopyTo(bytes, 0);
        await using var stream = new MemoryStream(bytes);
        if (valid) _uploader.Setup(provider => provider.UploadAsync(It.IsAny<UploadFileDTO>(), 7, default)).ReturnsAsync(Success());
        var result = await Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", stream, declaredSize), 7);
        Assert.Equal(valid ? StatusCodeResponse.Success : StatusCodeResponse.BadRequest, result.StatusCode);
        if (valid) _uploader.VerifyAll();
        else _uploader.VerifyNoOtherCalls();
        Assert.True(stream.Position <= 10485761);
    }

    [Fact]
    public async Task NonSeekableStream_IsBufferedFromTheBeginning()
    {
        await using var stream = new NonSeekableStream(Pdf);
        _uploader.Setup(provider => provider.UploadAsync(It.Is<UploadFileDTO>(file => file.Content.CanSeek && file.Content.Position == 0), 7, default))
            .Returns(async (UploadFileDTO file, int _, CancellationToken token) =>
            {
                using var copy = new MemoryStream();
                await file.Content.CopyToAsync(copy, token);
                Assert.Equal(Pdf, copy.ToArray());
                return Success();
            });
        Assert.Equal(StatusCodeResponse.Success, (await Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", stream, Pdf.Length), 7)).StatusCode);
        _uploader.VerifyAll();
    }

    [Fact]
    public async Task MissingFileOrStream_ReturnsValidationFailure()
    {
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadBusinessLicenseAsync(null!, 7)).StatusCode);
        Assert.Equal(StatusCodeResponse.BadRequest, (await Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", null!, 10), 7)).StatusCode);
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Cancellation_DoesNotBecomeServerErrorOrUpload()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        await using var stream = new MemoryStream(Pdf);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", stream, Pdf.Length), 7, cancellation.Token));
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CancellationDuringRead_DoesNotUploadOrDisposeCallerStream()
    {
        using var cancellation = new CancellationTokenSource();
        await using var stream = new CancelDuringReadStream(Pdf, cancellation);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", stream, Pdf.Length), 7, cancellation.Token));
        Assert.True(stream.CanRead);
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProviderCancellation_PropagatesAndDisposesBufferedStream()
    {
        await using var input = new MemoryStream(Pdf);
        Stream? captured = null;
        _uploader.Setup(provider => provider.UploadAsync(It.IsAny<UploadFileDTO>(), 7, default))
            .Callback<UploadFileDTO, int, CancellationToken>((file, _, _) => captured = file.Content)
            .ThrowsAsync(new OperationCanceledException());
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", input, Pdf.Length), 7));
        Assert.False(captured!.CanRead);
        Assert.True(input.CanRead);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task InvalidOwner_DoesNotUpload(int ownerId)
    {
        await using var input = new MemoryStream(Pdf);
        Assert.Equal(StatusCodeResponse.Unauthorized,
            (await Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", input, Pdf.Length), ownerId)).StatusCode);
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UnreadableContent_IsRejected()
    {
        var input = new MemoryStream(Pdf);
        await input.DisposeAsync();
        Assert.Equal(StatusCodeResponse.BadRequest,
            (await Service.UploadBusinessLicenseAsync(File("license.pdf", "application/pdf", input, Pdf.Length), 7)).StatusCode);
        _uploader.VerifyNoOtherCalls();
        _uploads.VerifyNoOtherCalls();
        _unit.VerifyNoOtherCalls();
    }

    private static UploadFileDTO File(string name, string mime, Stream stream, long size) =>
        new() { FileName = name, ContentType = mime, Content = stream, Size = size };
    private static ApiResponse<BusinessLicenseProviderUpload> Success() =>
        new() { StatusCode = StatusCodeResponse.Success, Content = new BusinessLicenseProviderUpload("asset/id", "image", "upload", "https://example.com/license.pdf") };

    private sealed class NonSeekableStream(byte[] bytes) : MemoryStream(bytes)
    {
        public override bool CanSeek => false;
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Length => throw new NotSupportedException();
    }

    private sealed class CancelDuringReadStream(byte[] bytes, CancellationTokenSource cancellation) : MemoryStream(bytes)
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellation.Cancel();
            return ValueTask.FromCanceled<int>(cancellationToken);
        }
    }
}
