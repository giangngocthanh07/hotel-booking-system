using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace HotelBooking.application.Services.Domains.Media;

public interface IBusinessLicenseUploader
{
    Task<ApiResponse<UploadResultDTO>> UploadAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken);
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

    public async Task<ApiResponse<UploadResultDTO>> UploadAsync(UploadFileDTO file, int userId, CancellationToken cancellationToken)
    {
        var isImage = Path.GetExtension(file.FileName).ToLowerInvariant() is ".jpg" or ".jpeg" or ".png";
        var parameters = CreateParameters(file, userId, isImage);
        var uploaded = await _cloudinary.UploadAsync(parameters, cancellationToken);
        if (uploaded.Error != null)
            return ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, MessageResponse.Common.UPDATE_FAILED);
        // Preserve the existing delivery contract; ownership/private retrieval are separate work.
        var url = isImage
            ? _cloudinary.Api.UrlImgUp.ResourceType("raw").Action("upload").Signed(true).BuildUrl(uploaded.PublicId)
            : uploaded.SecureUrl.ToString();
        return ResponseFactory.Success(new UploadResultDTO
        {
            Uploaded = true, FileName = file.FileName, StoredFileName = url,
        }, MessageResponse.Common.UPDATE_SUCCESSFULLY);
    }

    private static ImageUploadParams CreateParameters(UploadFileDTO file, int userId, bool isImage) => new()
    {
        File = new FileDescription(file.FileName, file.Content),
        PublicId = $"user_{userId}_business_license_{Guid.NewGuid()}",
        Folder = $"HotelBooking/Approvals/Pendings/user_{userId}/business_license",
        Transformation = isImage ? new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto") : null,
    };
}
