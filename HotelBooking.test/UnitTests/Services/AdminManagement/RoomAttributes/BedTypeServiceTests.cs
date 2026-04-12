
using FluentValidation;
using Moq;

namespace HotelBooking.test.UnitTests.Services.AdminManagement.RoomAttributes;

public class BedTypeServiceTests : BaseServiceTest
{
    private readonly Mock<IBedTypeRepository> _mockBedTypeRepo;
    private readonly Mock<IValidator<BedTypeCreateDTO>> _mockCreateValidator;
    private readonly Mock<IValidator<BedTypeUpdateDTO>> _mockUpdateValidator;
    private readonly Mock<IValidator<PagingRequest>> _mockPagingValidator;
    private readonly BedTypeService _bedTypeService;

    public BedTypeServiceTests()
    {
        _mockBedTypeRepo = new Mock<IBedTypeRepository>();
        _mockCreateValidator = new Mock<IValidator<BedTypeCreateDTO>>();
        _mockUpdateValidator = new Mock<IValidator<BedTypeUpdateDTO>>();
        _mockPagingValidator = new Mock<IValidator<PagingRequest>>();
        _bedTypeService = new BedTypeService(
            _mockBedTypeRepo.Object,
            _mockUnitOfWork.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockPagingValidator.Object
        );
    }
}
