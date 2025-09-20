using HotelBooking.infrastructure.Models;

public interface IHotelService
{
    Task<bool> ApproveHotelAsync(int hotelId, int adminId);
}

public class HotelService : IHotelService
{
    public HotelBookingContext _context;
    private readonly IHotelRepository _hotelRepository;
    public IUnitOfWork _dbu;

    public HotelService(HotelBookingContext context, IHotelRepository hotelRepository, IUnitOfWork dbu)
    {
        _context = context;
        _hotelRepository = hotelRepository;
        _dbu = dbu;
    }

    public async Task<bool> ApproveHotelAsync(int hotelId, int adminId)
    {
        var hotel = await _hotelRepository.GetByIdAsync(hotelId);
        if (hotel == null || hotel.Status != "PendingVerification")
            return false;

        hotel.Status = "Approved";
        hotel.IsVerified = true;
        hotel.ApprovedAt = DateTime.UtcNow;
        hotel.ApprovedBy = adminId;

        _hotelRepo.Update(hotel);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

}