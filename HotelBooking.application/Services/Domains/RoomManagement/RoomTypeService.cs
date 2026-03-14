using FluentValidation;
using HotelBooking.application.DTOs.Hotel;

namespace HotelBooking.application.Services.Domains.RoomManagement
{
    public interface IRoomTypeService
    {
        Task<ApiResponse<int>> CreateRoomTypeAsync(RoomTypeCreateDTO request);
    }

    public class RoomTypeService : IRoomTypeService
    {
        private readonly IRoomTypeRepository _roomTypeRepo;
        private readonly IRoomTypeBedConfigRepository _bedConfigRepo;
        private readonly IRoomAttributeFacade _attributeFacade;
        private readonly IValidator<RoomTypeCreateDTO> _validator;

        public RoomTypeService(IRoomTypeRepository roomTypeRepo, IRoomTypeBedConfigRepository bedConfigRepo, IValidator<RoomTypeCreateDTO> validator, IRoomAttributeFacade attributeFacade)
        {
            _roomTypeRepo = roomTypeRepo;
            _bedConfigRepo = bedConfigRepo;
            _validator = validator;
            _attributeFacade = attributeFacade;
        }
        public Task<ApiResponse<int>> CreateRoomTypeAsync(RoomTypeCreateDTO request)
        {
            throw new NotImplementedException(); // Implementation will be added in the next steps, focusing on validation and DB interaction
        }
    }
}