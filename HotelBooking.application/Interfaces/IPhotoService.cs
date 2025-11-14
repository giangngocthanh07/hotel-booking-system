namespace HotelBooking.application.Interfaces
{
    public interface IPhotoService
    {
        Task<string> UploadPhotoAsync(UploadFileDTO file, int userId);
        Task<string> UploadHotelCoverImageAsync(UploadFileDTO file, int userId, int hotelId);
        Task<string> UploadHotelMainImageAsync(UploadFileDTO file, int userId, int hotelId);
        Task<string> UploadHotelSubImageAsync(UploadFileDTO file, int userId, int hotelId);
    }
}