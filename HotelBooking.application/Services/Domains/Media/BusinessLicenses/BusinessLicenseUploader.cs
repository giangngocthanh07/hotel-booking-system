using HotelBooking.application.DTOs.Media.BusinessLicenses;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace HotelBooking.application.Services.Domains.Media.BusinessLicenses;

public interface IBusinessLicenseUploader
{
    Task<ApiResponse<BusinessLicenseProviderUpload>> UploadAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken);
}

// Keep provider behavior separate so validation can guarantee rejected files never reach Cloudinary.
public class BusinessLicenseUploader : IBusinessLicenseUploader
{
    private readonly ICloudinaryUploadApi _cloudinary;

    public BusinessLicenseUploader(IConfiguration configuration)
    {
        var cloudinary = new Cloudinary(new Account(configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"], configuration["Cloudinary:ApiSecret"]));
        cloudinary.Api.Secure = true;
        _cloudinary = cloudinary;
    }

    public BusinessLicenseUploader(ICloudinaryUploadApi cloudinary) => _cloudinary = cloudinary;

    public async Task<ApiResponse<BusinessLicenseProviderUpload>> UploadAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var uploaded = await _cloudinary.UploadAsync(CreateParameters(file, userId), null, cancellationToken);
        if (uploaded.Error != null)
            return ResponseFactory.Failure<BusinessLicenseProviderUpload>(StatusCodeResponse.BadRequest, MessageResponse.Common.UPDATE_FAILED);
        var metadata = new BusinessLicenseProviderUpload(uploaded.PublicId ?? string.Empty,
            uploaded.ResourceType ?? string.Empty, uploaded.Type ?? string.Empty, uploaded.SecureUrl?.ToString() ?? string.Empty);
        // FR-ONB-013: never register a public asset when private delivery was requested.
        if (metadata.ResourceType != "raw" || metadata.DeliveryType != "private" || !metadata.IsValid())
            return ResponseFactory.ServerError<BusinessLicenseProviderUpload>();
        return ResponseFactory.Success(metadata, MessageResponse.Common.UPDATE_SUCCESSFULLY);
    }

    // FR-ONB-013 / FR-MED-004: raw preserves the document; private prevents unsigned original access.
    private static RawUploadParams CreateParameters(UploadFileDTO file, int userId) => new()
    {
        File = new FileDescription(file.FileName, file.Content),
        PublicId = $"user_{userId}_business_license_{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}",
        Folder = $"HotelBooking/Approvals/Pendings/user_{userId}/business_license",
        Type = "private",
    };
}
