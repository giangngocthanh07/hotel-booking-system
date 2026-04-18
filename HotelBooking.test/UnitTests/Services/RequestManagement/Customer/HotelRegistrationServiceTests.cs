
using System.Linq.Expressions;
using FluentAssertions;
using FluentValidation;
using HotelBooking.application.Services.Domains.RequestManagement.Customer;
using HotelBooking.infrastructure.Models;
using Moq;

namespace HotelBooking.test.UnitTests.Services.RequestManagement.Customer;

public class HotelRegistrationServiceTests : BaseServiceTest
{
    private readonly Mock<IHotelRepository> _mockHotelRepo;
    private readonly Mock<IValidator<HotelRegistrationDTO>> _mockValidator;
    private readonly IHotelRegistrationService _service;


    public HotelRegistrationServiceTests()
    {
        _mockHotelRepo = new Mock<IHotelRepository>();
        _mockValidator = new Mock<IValidator<HotelRegistrationDTO>>();
        _service = new HotelRegistrationService(_mockHotelRepo.Object, _mockValidator.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task HotelRegistration_ValidRequest_ShouldReturnTrue()
    {
        // 1. Arrange
        var request = CreateValidRequest();

        // Mock validate success
        MockValidationSuccess();

        // Mock Name is not duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(false);

        // 2. Act
        var result = await _service.HotelRegistrationAsync(request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REQUEST_CREATED_SUCCESS);

        _mockHotelRepo.Verify(r => r.AddAsync(It.IsAny<Hotel>()), Times.Once);
        Verify_Saved(1);
    }

    [Fact]
    public async Task HotelRegistration_InvalidRequest_ShouldReturnBadRequest()
    {
        // 1. Arrange
        var request = CreateValidRequest();

        // Mock validate failure
        var validationFailures = new List<FluentValidation.Results.ValidationFailure>
        {
            new FluentValidation.Results.ValidationFailure("Name", MessageResponse.Validation.EMPTY_NAME)
        };

        _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<HotelRegistrationDTO>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

        // 2. Act
        var result = await _service.HotelRegistrationAsync(request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Message.Should().Be(validationFailures.First().ErrorMessage);

        Verify_Repo_Never_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_Never_AddAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task HotelRegistration_DuplicateName_ShouldReturnConflict()
    {
        // 1. Arrange
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock Name is duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(true);

        // 2. Act
        var result = await _service.HotelRegistrationAsync(request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
        result.Message.Should().Be(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_NAME_ALREADY_EXISTS);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_Never_AddAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task HotelRegistration_SystemThrowsExceptionAtAnyAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock throw exception at AnyAsync
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.HotelRegistrationAsync(request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_Never_AddAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task HotelRegistration_SystemThrowsExceptionAtAddAsync_ShouldReturnServerError()
    {
        // 1. Arrange
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock Name is not duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(false);

        // Mock throw exxception at AddAsync
        _mockHotelRepo.Setup(x => x.AddAsync(It.IsAny<Hotel>()))
            .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.HotelRegistrationAsync(request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_AddAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Never_Saved();
    }

    [Fact]
    public async Task HotelRegistration_SaveDbFails_ShouldReturnError()
    {
        // 1. Arrange
        var request = CreateValidRequest();

        MockValidationSuccess();

        // Mock Name is not duplicate
        _mockHotelRepo.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
            .ReturnsAsync(false);

        // Mock save db fails
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

        // 2. Act
        var result = await _service.HotelRegistrationAsync(request);

        // 3. Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodeResponse.Error);
        result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);

        // Verify
        Verify_Repo_AnyAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Repo_AddAsync<IHotelRepository, Hotel>(_mockHotelRepo);
        Verify_Saved(1);
    }



    private HotelRegistrationDTO CreateValidRequest()
    {
        return new HotelRegistrationDTO
        {
            Name = "Test Hotel",
            Address = "123 Test Street",
            Description = "Description 1",
            PropertyTypeId = 1,
            StarRating = 3,
            PublicPhone = "0123456789",
            PublicEmail = "testhotel@gmail.com",
            CountryId = 4,
            ProvinceId = 1,
            WardId = 1,
            Latitude = 10.0,
            Longitude = 20.0,
            TaxCode = "1234567890",
            BusinessLicenseUrl = "https://example.com/license.pdf"
        };
    }

    private void MockValidationSuccess()
    {
        _mockValidator.Setup(m => m.ValidateAsync(It.IsAny<HotelRegistrationDTO>(), default)).ReturnsAsync(new FluentValidation.Results.ValidationResult());

    }

}