using HotelBooking.application.Validators.Media;

namespace HotelBooking.application.Services.Domains.Media;

public interface IBusinessLicenseAccessService
{
    Task<ApiResponse<BusinessLicenseDocument>> DownloadAsync(BusinessLicenseAccessRequest access, CancellationToken cancellationToken = default);
}

public class BusinessLicenseAccessService(IBusinessLicenseUploadRepository uploads, IBusinessLicenseDownloader downloader)
    : IBusinessLicenseAccessService
{
    private readonly BusinessLicenseAccessValidator _validator = new();
    private readonly BusinessLicenseDownloadValidator _sourceValidator = new();

    public async Task<ApiResponse<BusinessLicenseDocument>> DownloadAsync(BusinessLicenseAccessRequest access,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (access == null || !(await _validator.ValidateAsync(access, cancellationToken)).IsValid)
            return ResponseFactory.Failure<BusinessLicenseDocument>(StatusCodeResponse.BadRequest, "Invalid document request.");
        // FR-ONB-013/014: scope the SQL lookup before provider access; never fall back to legacy public URLs.
        var upload = await uploads.GetForRequestAsync(access.RequestId, access.IsAdmin ? null : access.UserId, cancellationToken);
        if (upload == null || upload.IsDeleted || (!access.IsAdmin && upload.OwnerId != access.UserId)) return Missing();
        var source = new BusinessLicenseDownloadSource(upload.PublicId, upload.ResourceType, upload.DeliveryType, upload.ContentType, upload.SizeBytes);
        if (!(await _sourceValidator.ValidateAsync(source, cancellationToken)).IsValid) return Missing();
        var downloaded = await downloader.DownloadAsync(source, cancellationToken);
        if (downloaded.StatusCode != StatusCodeResponse.Success)
            return ResponseFactory.Failure<BusinessLicenseDocument>(downloaded.StatusCode, downloaded.Message);
        if (downloaded.Content == null) return ResponseFactory.ServerError<BusinessLicenseDocument>();
        return ResponseFactory.Success(new BusinessLicenseDocument(downloaded.Content, source.ContentType,
            $"business-license-{access.RequestId}{Path.GetExtension(source.PublicId).ToLowerInvariant()}"), "Business license retrieved.");
    }

    private static ApiResponse<BusinessLicenseDocument> Missing() =>
        ResponseFactory.Failure<BusinessLicenseDocument>(StatusCodeResponse.NotFound, "Business license is unavailable.");
}
