using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HotelBooking.application.Services.Domains.Media;

namespace HotelBooking.application.Services.Domains.Media
{
    public interface IPhotoService
    {
        Task<string> UploadPhotoAsync(UploadFileDTO file, int userId);
        Task<string> UploadHotelCoverImageAsync(UploadFileDTO file, int userId, int hotelId);
        Task<string> UploadHotelMainImageAsync(UploadFileDTO file, int userId, int hotelId);
        Task<string> UploadHotelSubImageAsync(UploadFileDTO file, int userId, int hotelId);
    }
}
public class PhotoService : IPhotoService
{
    private readonly Cloudinary _cloudinary;

    public PhotoService(IConfiguration Configuration)
    {
        var cloudName = Configuration["Cloudinary:CloudName"];
        var apiKey = Configuration["Cloudinary:ApiKey"];
        var apiSecret = Configuration["Cloudinary:ApiSecret"];


        var acc = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(acc);
        _cloudinary.Api.Secure = true;
    }

    public async Task<string> UploadPhotoAsync(UploadFileDTO file, int userId)
    {
        var uploadResult = new ImageUploadResult();

        if (file.Size > 0)
        {

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.Content),
                Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
                PublicId = $"user_{userId}_{Guid.NewGuid()}",
                Folder = $"HotelBooking/user_{userId}"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);

        }

        if (uploadResult.Error != null)
        {
            return null;
        }

        return file.FileName;
    }

    public async Task<string> UploadHotelCoverImageAsync(UploadFileDTO file, int userId, int hotelId)
    {
        var uploadResult = new ImageUploadResult();

        if (file != null && file.Size > 0)
        {

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.Content),
                Transformation = new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto"),
                PublicId = $"hotel_{hotelId}_cover_{Guid.NewGuid()}",
                Folder = $"HotelBooking/Hotels/user_{userId}/hotel_{hotelId}/cover"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);

        }

        if (uploadResult.Error != null)
        {
            return null;
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> UploadHotelMainImageAsync(UploadFileDTO file, int userId, int hotelId)
    {
        var uploadResult = new ImageUploadResult();

        if (file != null && file.Size > 0)
        {

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.Content),
                Transformation = new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto"),
                PublicId = $"hotel_{hotelId}_main_{Guid.NewGuid()}",
                Folder = $"HotelBooking/Hotels/user_{userId}/hotel_{hotelId}/main"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);

        }

        if (uploadResult.Error != null)
        {
            return null;
        }

        return uploadResult.SecureUrl.ToString();
    }

    public async Task<string> UploadHotelSubImageAsync(UploadFileDTO file, int userId, int hotelId)
    {
        var uploadResult = new ImageUploadResult();

        if (file != null && file.Size > 0)
        {

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, file.Content),
                Transformation = new Transformation().Width(800).Height(600).Crop("fill").Gravity("auto"),
                PublicId = $"hotel_{hotelId}_sub_{Guid.NewGuid()}",
                Folder = $"HotelBooking/Hotels/user_{userId}/hotel_{hotelId}/sub"
            };

            uploadResult = await _cloudinary.UploadAsync(uploadParams);

        }

        if (uploadResult.Error != null)
        {
            return null;
        }

        return uploadResult.SecureUrl.ToString();
    }
}





