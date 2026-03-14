using HotelBooking.infrastructure.Models;
using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.application.DTOs.Hotel;

using Moq;
using FluentValidation;

namespace HotelBooking.Tests.Services.RoomManagement
{
    public class RoomManagementServiceTest : BaseServiceTest
    {
        private readonly Mock<IRoomTypeRepository> _mockRoomTypeRepo;
        private readonly Mock<IRoomTypeBedConfigRepository> _mockBedConfigRepo;
        private readonly Mock<IRoomAttributeFacade> _mockAttributeFacade;
        private readonly Mock<IValidator<RoomTypeCreateDTO>> _mockValidator;
        private readonly RoomTypeService _service;

        public RoomManagementServiceTest()
        {
            _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
            _mockBedConfigRepo = new Mock<IRoomTypeBedConfigRepository>();
            _mockAttributeFacade = new Mock<IRoomAttributeFacade>();
            _mockValidator = new Mock<IValidator<RoomTypeCreateDTO>>();
            _service = new RoomTypeService(_mockRoomTypeRepo.Object, _mockBedConfigRepo.Object, _mockValidator.Object, _mockAttributeFacade.Object);
        }

        

    }
}