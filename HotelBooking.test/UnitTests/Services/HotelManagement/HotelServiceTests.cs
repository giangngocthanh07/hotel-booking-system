using Moq;
using HotelBooking.application.Services.Domains.HotelManagement;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Validators.RoomManagement;
using FluentValidation;
using FluentValidation.Results;
using HotelBooking.application.Services.Domains.Media;
using Microsoft.Extensions.Logging;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.test.UnitTests.Services.HotelManagement;

public class HotelServiceTests : BaseServiceTest<HotelService>
{
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly Mock<IHotelImageRepository> _mockHotelImageRepo;
    private readonly Mock<IHotelAmenityRepository> _mockHotelAmenityRepo;
    private readonly Mock<IHotelPolicyRepository> _mockHotelPolicyRepo;
    private readonly Mock<IPropertyTypeRepository> _mockPropTypeRepo;
    private readonly Mock<IImageHelper> _mockImageHelper;
    private readonly Mock<IPhotoService> _mockPhotoService;
    private readonly Mock<IValidator<HotelSearchRequestDTO>> _mockValidator;
    private readonly HotelService _service;

    public HotelServiceTests()
    {
        _mockHotelRepo = new Mock<IHotelRepository>();
        _mockHotelImageRepo = new Mock<IHotelImageRepository>();
        _mockHotelAmenityRepo = new Mock<IHotelAmenityRepository>();
        _mockHotelPolicyRepo = new Mock<IHotelPolicyRepository>();
        _mockPropTypeRepo = new Mock<IPropertyTypeRepository>();
        _mockImageHelper = new Mock<IImageHelper>();
        _mockPhotoService = new Mock<IPhotoService>();
        _mockValidator = new Mock<IValidator<HotelSearchRequestDTO>>();

        _service = new HotelService(
            _mockHotelRepo.Object,
            _mockHotelImageRepo.Object,
            _mockHotelAmenityRepo.Object,
            _mockHotelPolicyRepo.Object,
            _mockPropTypeRepo.Object,
            _mockImageHelper.Object,
            _mockPhotoService.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockValidator.Object
        );
    }

    [Fact]
    public async Task SearchHotelsAsync_InvalidRequest_ReturnsError()
    {
        // Arrange
        var request = new HotelSearchRequestDTO();
        var validationFailures = new List<ValidationFailure> { new ValidationFailure("CityName", "Vui lòng nhập tên thành phố.") };
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // Act
        var result = await _service.SearchHotelsAsync(request);

        // Assert
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Contains("Vui lòng nhập tên thành phố.", result.Message);
        _mockHotelRepo.Verify(r => r.GetSearchHotelsAsync(It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task SearchHotelsAsync_ValidRequest_ReturnsHotels()
    {
        // Arrange
        var request = new HotelSearchRequestDTO { CityName = "Hanoi", CheckIn = DateTime.Today.AddDays(1), CheckOut = DateTime.Today.AddDays(3) };
        _mockValidator.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var searchResults = new List<SearchHotelResult>
        {
            new SearchHotelResult { Id = 1, Name = "Hotel 1", CityName = "Hanoi", PriceFrom = 100, AvailableRooms = 5 }
        };

        _mockHotelRepo.Setup(r => r.GetSearchHotelsAsync(request.CityName, request.CheckIn, request.CheckOut, request.Adults, request.Children, request.Rooms))
            .ReturnsAsync(searchResults);

        // Act
        var result = await _service.SearchHotelsAsync(request);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.NotNull(result.Content);
        Assert.Single(result.Content);
        Assert.Equal("Hotel 1", result.Content.First().Name);
    }

    [Fact]
    public async Task GetHotelDetailsAsync_HotelExists_ReturnsDetails()
    {
        // Arrange
        int hotelId = 1;
        var hotel = new Hotel
        {
            Id = hotelId,
            Name = "Luxury Hotel",
            HotelImages = new List<HotelImage>(),
            HotelAmenities = new List<HotelAmenity>(),
            RoomTypes = new List<RoomType>(),
            Reviews = new List<Review>()
        };

        _mockHotelRepo.Setup(r => r.GetHotelDetailsByIdAsync(hotelId)).ReturnsAsync(hotel);

        // Act
        var result = await _service.GetHotelDetailsAsync(hotelId);

        // Assert
        Assert.Equal(StatusCodeResponse.Success, result.StatusCode);
        Assert.Equal("Luxury Hotel", result.Content.Name);
    }

    [Fact]
    public async Task GetHotelDetailsAsync_HotelNotFound_ReturnsError()
    {
        // Arrange
        int hotelId = 999;
        _mockHotelRepo.Setup(r => r.GetHotelDetailsByIdAsync(hotelId)).ReturnsAsync((Hotel?)null);

        // Act
        var result = await _service.GetHotelDetailsAsync(hotelId);

        // Assert
        Assert.Equal(StatusCodeResponse.Error, result.StatusCode);
        Assert.Null(result.Content);
    }
}
