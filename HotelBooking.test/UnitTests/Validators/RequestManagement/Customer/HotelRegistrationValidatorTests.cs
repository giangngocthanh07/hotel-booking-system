
using FluentAssertions;
using FluentValidation.TestHelper;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Validators.RequestManagement.Owner;

namespace HotelBooking.test.UnitTests.Validators.RequestManagement.Customer;

public class HotelRegistrationValidatorTests
{
    HotelRegistrationValidator _validator;
    public HotelRegistrationValidatorTests()
    {
        _validator = new HotelRegistrationValidator();
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var request = CreateValidRequest();

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_InvalidName_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.Name = "";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage(MessageResponse.Validation.EMPTY_NAME);
    }

    [Fact]
    public async Task ValidateAsync_ShortName_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.Name = "A";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.Validation.SHORT_NAME);
    }

    [Fact]
    public async Task ValidateAsync_LongName_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.Name = new string('a', 51);

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Name).WithErrorMessage(MessageResponse.Validation.LONG_NAME);
    }

    [Fact]
    public async Task ValidateAsync_InvalidAddress_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.Address = "";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Address).WithErrorMessage(MessageResponse.Validation.EMPTY_ADDRESS);
    }

    [Fact]
    public async Task ValidateAsync_ShortAddress_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.Address = "A";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage(MessageResponse.Validation.SHORT_ADDRESS);
    }

    [Fact]
    public async Task ValidateAsync_LongAddress_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.Address = new string('a', 501);

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage(MessageResponse.Validation.LONG_ADDRESS);
    }


    [Fact]
    public async Task ValidateAsync_LongDescription_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.Description = new string('a', 501);

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Description).WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_InvalidPropertyTypeId_ReturnsError(int invalidTypeId)
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.PropertyTypeId = invalidTypeId;

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PropertyTypeId).WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_PROPERTY_TYPEID);
    }

    [Fact]
    public async Task ValidateAsync_NullStarRating_ReturnsSuccess()
    {
        var request = CreateValidRequest();
        request.StarRating = null;

        var result = await _validator.TestValidateAsync(request);

        result.IsValid.Should().BeTrue();
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task ValidateAsync_InvalidStarRating_ReturnsError(int invalidRating)
    {
        var request = CreateValidRequest();
        request.StarRating = invalidRating;

        var result = await _validator.TestValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.StarRating)
              .WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_STARRATING);
    }

    [Fact]
    public async Task ValidateAsync_EmptyPublicPhone_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.PublicPhone = "";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PublicPhone).WithErrorMessage(MessageResponse.Validation.EMPTY_PHONE_NUMBER);
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("12345678an")]
    [InlineData("12345678901")]
    public async Task ValidateAsync_InvalidPublicPhone_ReturnsError(string invalidPhone)
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.PublicPhone = invalidPhone;

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PublicPhone).WithErrorMessage(MessageResponse.Validation.INVALID_PHONE_NUMBER);
    }

    [Fact]
    public async Task ValidateAsync_EmptyPublicEmail_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.PublicEmail = "";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PublicEmail)
            .WithErrorMessage(MessageResponse.Validation.EMPTY_EMAIL);
    }

    [Fact]
    public async Task ValidateAsync_InvalidPublicEmail_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.PublicEmail = "testhotelgmail.com";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.PublicEmail).WithErrorMessage(MessageResponse.Validation.INVALID_EMAIL_FORMAT);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_InvalidProvinceId_ReturnsError(int invalidProvinceId)
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.ProvinceId = invalidProvinceId;

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.ProvinceId).WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_PROVINCE_ID);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ValidateAsync_InvalidWardId_ReturnsError(int invalidWardId)
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.WardId = invalidWardId;

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.WardId)
            .WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_WARD_ID);
    }

    [Theory]
    [InlineData(-91.0)]
    [InlineData(91.0)]
    public async Task ValidateAsync_InvalidLatitude_ReturnsError(double invalidLat)
    {
        var request = CreateValidRequest();
        request.Latitude = invalidLat;

        var result = await _validator.TestValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Latitude)
              .WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_LATITUDE);
    }

    [Theory]
    [InlineData(-181.0)]
    [InlineData(181.0)]
    public async Task ValidateAsync_InvalidLongitude_ReturnsError(double invalidLng)
    {
        var request = CreateValidRequest();
        request.Longitude = invalidLng;

        var result = await _validator.TestValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.Longitude)
              .WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_LONGITUDE);
    }

    [Fact]
    public async Task ValidateAsync_EmptyTaxCode_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.TaxCode = "";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TaxCode).WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_EMPTY_TAX_CODE);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("123456789012")]
    [InlineData("12345678901234")]
    public async Task ValidateAsync_InvalidTaxCode_ReturnsError(string invalidTaxCode)
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.TaxCode = invalidTaxCode;

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.TaxCode).WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_TAX_CODE);
    }

    [Fact]
    public async Task ValidateAsync_EmptyBusinessLicenseUrl_ReturnsError()
    {
        // 1. Arrange
        var request = CreateValidRequest();
        request.BusinessLicenseUrl = "";

        // 2. Act
        var result = await _validator.TestValidateAsync(request);

        // 3. Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.BusinessLicenseUrl)
            .WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_EMPTY_BUSINESS_LICENSE_URL);
    }

    [Fact]
    public async Task ValidateAsync_InvalidBusinessLicenseUrl_ReturnsError()
    {
        var request = CreateValidRequest();
        request.BusinessLicenseUrl = "not-a-valid-url"; // Test format URL

        var result = await _validator.TestValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.BusinessLicenseUrl)
              .WithErrorMessage(MessageResponse.RequestManagement.HotelApproval.HOTEL_REGISTRATION_INVALID_BUSINESS_LICENSE_URL);
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
}