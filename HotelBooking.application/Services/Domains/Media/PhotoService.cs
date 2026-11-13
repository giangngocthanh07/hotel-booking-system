using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HotelBooking.application.Services.Domains.Media;

namespace HotelBooking.application.Services.Domains.Media
{
    public interface IPhotoService
    {
        Task<ApiResponse<UploadResultDTO>> UploadPhotoAsync(UploadFileDTO file, int userId);
        Task<ApiResponse<UploadResultDTO>> UploadHotelCoverImageAsync(UploadFileDTO file, int userId, int hotelId);
        Task<ApiResponse<UploadResultDTO>> UploadHotelMainImageAsync(UploadFileDTO file, int userId, int hotelId);
        Task<ApiResponse<UploadResultDTO>> UploadHotelSubImageAsync(UploadFileDTO file, int userId, int hotelId);
        Task<ApiResponse<UploadResultDTO>> UploadHotelImageAsync(UploadFileDTO file, int ownerId, int hotelId, bool cover, CancellationToken cancellationToken = default);
    }
}

public class PhotoService : IPhotoService
{
    private readonly ICloudinaryUploadApi _cloudinary;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(IConfiguration configuration, ILogger<PhotoService> logger) : this(CreateClient(configuration), logger) { }

    public PhotoService(ICloudinaryUploadApi cloudinary, ILogger<PhotoService> logger)
    {
        _cloudinary = cloudinary;
        _logger = logger;
    }

    private static Cloudinary CreateClient(IConfiguration configuration)
    {
        var cloudinary = new Cloudinary(new Account(configuration["Cloudinary:CloudName"], configuration["Cloudinary:ApiKey"], configuration["Cloudinary:ApiSecret"]));
        cloudinary.Api.Secure = true;
        return cloudinary;
    }

    private async Task<ApiResponse<UploadResultDTO>> ExecuteUploadAsync(UploadFileDTO file, ImageUploadParams uploadParams,
        CancellationToken cancellationToken = default, bool requirePublicImage = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var uploaded = await _cloudinary.UploadAsync(uploadParams, cancellationToken);
            if (uploaded.Error != null) _logger.LogError("Cloudinary upload error: {Error}", uploaded.Error.Message);
            return MapUpload(file, uploaded, requirePublicImage);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading to Cloudinary");
            return ResponseFactory.ServerError<UploadResultDTO>();
        }
    }

    private static ApiResponse<UploadResultDTO> MapUpload(UploadFileDTO file, ImageUploadResult uploaded, bool requirePublicImage)
    {
        if (uploaded.Error != null)
            return ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, MessageResponse.Common.UPDATE_FAILED);
        if (uploaded.SecureUrl == null || (requirePublicImage && (uploaded.ResourceType != "image" || uploaded.Type != "upload")))
            return ResponseFactory.ServerError<UploadResultDTO>();
        return ResponseFactory.Success(new UploadResultDTO
        {
            Uploaded = true, FileName = file.FileName, StoredFileName = uploaded.SecureUrl.ToString(),
        }, MessageResponse.Common.UPDATE_SUCCESSFULLY);
    }

    public Task<ApiResponse<UploadResultDTO>> UploadHotelImageAsync(UploadFileDTO file, int ownerId, int hotelId, bool cover,
        CancellationToken cancellationToken = default)
    {
        // FR-MED-001/002/004: retain original proportions; presentation chooses how to crop the image.
        var kind = cover ? "cover" : "gallery";
        var parameters = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, file.Content), Type = "upload", Overwrite = false,
            PublicId = $"hotel_{hotelId}_{kind}_{Guid.NewGuid():N}",
            Folder = $"HotelBooking/Hotels/user_{ownerId}/hotel_{hotelId}/{kind}",
        };
        return ExecuteUploadAsync(file, parameters, cancellationToken, requirePublicImage: true);
    }

    public async Task<ApiResponse<UploadResultDTO>> UploadPhotoAsync(UploadFileDTO file, int userId)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.Content),
            Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
            PublicId = $"user_{userId}_{Guid.NewGuid()}",
            Folder = $"HotelBooking/user_{userId}"
        };

        return await ExecuteUploadAsync(file, uploadParams);
    }

    public async Task<ApiResponse<UploadResultDTO>> UploadHotelCoverImageAsync(UploadFileDTO file, int userId, int hotelId)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.Content),
            Transformation = new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto"),
            PublicId = $"hotel_{hotelId}_cover_{Guid.NewGuid()}",
            Folder = $"HotelBooking/Hotels/user_{userId}/hotel_{hotelId}/cover"
        };

        return await ExecuteUploadAsync(file, uploadParams);
    }

    public async Task<ApiResponse<UploadResultDTO>> UploadHotelMainImageAsync(UploadFileDTO file, int userId, int hotelId)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.Content),
            Transformation = new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto"),
            PublicId = $"hotel_{hotelId}_main_{Guid.NewGuid()}",
            Folder = $"HotelBooking/Hotels/user_{userId}/hotel_{hotelId}/main"
        };

        return await ExecuteUploadAsync(file, uploadParams);
    }

    public async Task<ApiResponse<UploadResultDTO>> UploadHotelSubImageAsync(UploadFileDTO file, int userId, int hotelId)
    {
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.Content),
            Transformation = new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto"),
            PublicId = $"hotel_{hotelId}_sub_{Guid.NewGuid()}",
            Folder = $"HotelBooking/Hotels/user_{userId}/hotel_{hotelId}/sub"
        };

        return await ExecuteUploadAsync(file, uploadParams);
    }
}





