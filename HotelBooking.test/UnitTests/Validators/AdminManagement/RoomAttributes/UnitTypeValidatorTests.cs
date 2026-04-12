using FluentValidation.TestHelper;
using HotelBooking.application.Validators.AdminManagement.RoomAttributes;

namespace HotelBooking.test.UnitTests.Validators.AdminManagement.RoomAttributes;

public class UnitTypeCreateValidatorTests
{
    private readonly UnitTypeCreateValidator _createValidator;

    public UnitTypeCreateValidatorTests()
    {
        _createValidator = new UnitTypeCreateValidator();
    }

    [Fact]
    public async Task CreateValidate_ValidRequest_IsEntirePlace_True_ReturnsSuccess()
    {
        // 1. Arrange
        var createDTO = new UnitTypeCreateDTO
        {
            Name = "Entire Apartment",
            IsEntirePlace = true,
            Description = "The whole place is rented"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task CreateValidate_ValidRequest_IsEntirePlace_False_ReturnsSuccess()
    {
        // 1. Arrange
        var createDTO = new UnitTypeCreateDTO
        {
            Name = "Private Room",
            IsEntirePlace = false,
            Description = "A private room within a shared property"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task CreateValidate_EmptyName_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new UnitTypeCreateDTO
        {
            Name = "",
            IsEntirePlace = true,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME);
    }

    [Fact]
    public async Task CreateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new UnitTypeCreateDTO
        {
            Name = new string('A', 51),
            IsEntirePlace = false,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.LONG_NAME);
    }

    [Fact]
    public async Task CreateValidate_LongDescription_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new UnitTypeCreateDTO
        {
            Name = "Entire Apartment",
            IsEntirePlace = true,
            Description = new string('A', 501)
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }

    [Fact]
    public async Task CreateValidate_NullDescription_ReturnsSuccess()
    {
        // 1. Arrange — Description is optional
        var createDTO = new UnitTypeCreateDTO
        {
            Name = "Entire Apartment",
            IsEntirePlace = true,
            Description = null
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}

public class UnitTypeUpdateValidatorTests
{
    private readonly UnitTypeUpdateValidator _updateValidator;

    public UnitTypeUpdateValidatorTests()
    {
        _updateValidator = new UnitTypeUpdateValidator();
    }

    [Fact]
    public async Task UpdateValidate_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var updateDTO = new UnitTypeUpdateDTO
        {
            Name = "Shared Dormitory",
            Description = "Shared space with multiple beds"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task UpdateValidate_EmptyName_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new UnitTypeUpdateDTO
        {
            Name = "",
            Description = "Description"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.EMPTY_NAME);
    }

    [Fact]
    public async Task UpdateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new UnitTypeUpdateDTO
        {
            Name = new string('A', 51),
            Description = "Description"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.UnitType.LONG_NAME);
    }

    [Fact]
    public async Task UpdateValidate_LongDescription_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new UnitTypeUpdateDTO
        {
            Name = "Shared Dormitory",
            Description = new string('A', 501)
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }

    [Fact]
    public async Task UpdateValidate_NullDescription_ReturnsSuccess()
    {
        // 1. Arrange — Description is optional
        var updateDTO = new UnitTypeUpdateDTO
        {
            Name = "Shared Dormitory",
            Description = null
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
