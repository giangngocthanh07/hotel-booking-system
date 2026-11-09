using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace HotelBooking.application.Services.Domains.Media;

public interface IBusinessLicenseUploader
{
    Task<ApiResponse<BusinessLicenseProviderUpload>> UploadAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken);
}

// Keep provider behavior separate so validation can guarantee rejected files never reach Cloudinary.
public class BusinessLicenseUploader : IBusinessLicenseUploader
{
    private readonly Cloudinary _cloudinary;

    public BusinessLicenseUploader(IConfiguration configuration)
    {
        _cloudinary = new Cloudinary(new Account(configuration["Cloudinary:CloudName"],
            configuration["Cloudinary:ApiKey"], configuration["Cloudinary:ApiSecret"]));
        _cloudinary.Api.Secure = true;
    }

    public async Task<ApiResponse<BusinessLicenseProviderUpload>> UploadAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken)
    {
        var isImage = Path.GetExtension(file.FileName).ToLowerInvariant() is ".jpg" or ".jpeg" or ".png";
        var parameters = CreateParameters(file, userId, isImage);
        var uploaded = await _cloudinary.UploadAsync(parameters, cancellationToken);
        if (uploaded.Error != null)
            return ResponseFactory.Failure<BusinessLicenseProviderUpload>(StatusCodeResponse.BadRequest, MessageResponse.Common.UPDATE_FAILED);
        // FR-ONB-013: record actual provider metadata and its canonical URL, not a client URL
        // or a synthesized raw URL for an image asset. Private delivery remains separate work.
        return ResponseFactory.Success(new BusinessLicenseProviderUpload(uploaded.PublicId ?? string.Empty,
            uploaded.ResourceType ?? string.Empty, uploaded.Type ?? string.Empty, uploaded.SecureUrl?.ToString() ?? string.Empty),
            MessageResponse.Common.UPDATE_SUCCESSFULLY);
    }

    private static ImageUploadParams CreateParameters(UploadFileDTO file, int userId, bool isImage) => new()
    {
        File = new FileDescription(file.FileName, file.Content),
        PublicId = $"user_{userId}_business_license_{Guid.NewGuid()}",
        Folder = $"HotelBooking/Approvals/Pendings/user_{userId}/business_license",
        Transformation = isImage ? new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto") : null,
    };
}
