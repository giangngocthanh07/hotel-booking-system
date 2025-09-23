using HotelBooking.infrastructure.Models;

public interface IHotelService
{
    public Task<string> GetOwnerDashBoard(int ownerId);
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

    public async Task<string> GetOwnerDashBoard(int ownerId)
    {
        return await Task.FromResult($"Owner Dashboard for Owner ID: {ownerId}");
    }

}