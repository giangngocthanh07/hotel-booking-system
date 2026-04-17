
using HotelBooking.application.Services.Domains.RequestManagement.Customer;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Customer;

public class HotelRegistrationServiceTests : BaseServiceTest
{
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly IHotelRegistrationService _service;


    public HotelRegistrationServiceTests()
    {
        _mockHotelRepo = new Mock<IHotelRepository>();
        _service = new HotelRegistrationService(_mockHotelRepo.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task HotelRegistration_ValidRequest_ShouldReturnTrue()
    {
        // 1. Arrange
    }

}