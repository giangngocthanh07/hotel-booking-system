using HotelBooking.application.Services.Helpers.Media.BusinessLicenses;
using HotelBooking.application.Validators.Media.BusinessLicenses;
using HotelBooking.application.DTOs.Media.BusinessLicenses;
using CloudinaryDotNet;

namespace HotelBooking.application.Services.Domains.Media.BusinessLicenses;

public interface IBusinessLicenseDownloader
{
    Task<ApiResponse<byte[]>> DownloadAsync(BusinessLicenseDownloadSource source, CancellationToken cancellationToken = default);
}

// FR-ONB-013/014: low-level provider access; the calling use case must authorize the request first.
public class BusinessLicenseDownloader : IBusinessLicenseDownloader
{
    private readonly HttpClient _client;
    private readonly Cloudinary _cloudinary;
    private readonly BusinessLicenseDownloadValidator _validator = new();

    public BusinessLicenseDownloader(HttpClient client, IConfiguration configuration)
    {
        _client = client;
        _cloudinary = new Cloudinary(new Account(configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"], configuration["Cloudinary:ApiSecret"]));
        _cloudinary.Api.Secure = true;
    }

    public async Task<ApiResponse<byte[]>> DownloadAsync(BusinessLicenseDownloadSource source, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (source == null || !(await _validator.ValidateAsync(source, cancellationToken)).IsValid)
            return ResponseFactory.Failure<byte[]>(StatusCodeResponse.BadRequest, "Business license is unavailable.");
        try { return await FetchAsync(source, cancellationToken); }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        // Signed URLs can appear in transport exceptions. Never return or log their details.
        catch (HttpRequestException) { return Unavailable(); }
        catch (IOException) { return Unavailable(); }
        catch (OperationCanceledException) { return Unavailable(); }
    }

    private async Task<ApiResponse<byte[]>> FetchAsync(BusinessLicenseDownloadSource source, CancellationToken cancellationToken)
    {
        var expires = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds();
        var signed = _cloudinary.DownloadPrivate(source.PublicId, false, null, "private", expires, "raw");
        // No StoredUrl is used. Restrict the SDK-generated destination and never follow redirects.
        if (!Uri.TryCreate(signed, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.Host != "api.cloudinary.com")
            return Unavailable();
        using var response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode) return Unavailable();
        if (response.Content.Headers.ContentLength is { } length && length != source.SizeBytes) return Unavailable();
        return await ReadAsync(response.Content, source, cancellationToken);
    }

    private static async Task<ApiResponse<byte[]>> ReadAsync(HttpContent response, BusinessLicenseDownloadSource source,
        CancellationToken cancellationToken)
    {
        await using var stream = await response.ReadAsStreamAsync(cancellationToken);
        await using var content = await BusinessLicenseFileRules.ReadContentAsync(stream, cancellationToken);
        if (content == null || content.Length != source.SizeBytes ||
            !BusinessLicenseFileRules.MatchesSignature(source.PublicId, content.GetBuffer().AsSpan(0, (int)content.Length)))
            return Unavailable();
        return ResponseFactory.Success(content.ToArray(), "Business license retrieved.");
    }

    private static ApiResponse<byte[]> Unavailable() =>
        ResponseFactory.Failure<byte[]>(StatusCodeResponse.Error, "Business license is unavailable.");
}
