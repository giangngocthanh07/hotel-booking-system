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
    }
}

public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<PhotoService> _logger;

    public PhotoService(IConfiguration Configuration, ILogger<PhotoService> logger)
    {
        var cloudName = Configuration["Cloudinary:CloudName"];
        var apiKey = Configuration["Cloudinary:ApiKey"];
        var apiSecret = Configuration["Cloudinary:ApiSecret"];

        var acc = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(acc);
        _cloudinary.Api.Secure = true;

        _logger = logger;
    }

    private async Task<ApiResponse<UploadResultDTO>> ExecuteUploadAsync(UploadFileDTO file, ImageUploadParams uploadParams)
    {
        try
        {
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            
            if (uploadResult.Error != null)
            {
                _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                return ResponseFactory.Failure<UploadResultDTO>(StatusCodeResponse.BadRequest, MessageResponse.Common.UPDATE_FAILED);
            }

            var result = new UploadResultDTO
            {
                Uploaded = true,
                FileName = file.FileName,
                StoredFileName = uploadResult.SecureUrl.ToString()
            };

            return ResponseFactory.Success(result, MessageResponse.Common.UPDATE_SUCCESSFULLY);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading to Cloudinary");
            return ResponseFactory.ServerError<UploadResultDTO>();
        }
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





