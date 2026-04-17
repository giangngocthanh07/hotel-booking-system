
namespace HotelBooking.application.Services.Domains.RequestManagement.Customer
{
    public interface IHotelRegistrationService
    {
    }

    public class HotelRegistrationService : IHotelRegistrationService
    {
        private readonly IHotelRepository _hotelRepo;
        private readonly IUnitOfWork _unitOfWork;

        public HotelRegistrationService(IHotelRepository hotelRepo, IUnitOfWork unitOfWork)
        {
            _hotelRepo = hotelRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task HotelRegistrationAsync(HotelRegistrationDTO request)
        {
            throw new NotImplementedException();
        }
    }
}