using FluentValidation.TestHelper;
using HotelBooking.application.Validators.AdminManagement.RoomAttributes;

namespace HotelBooking.test.UnitTests.Validators.AdminManagement.RoomAttributes;

public class RoomQualityCreateValidatorTests
{
    private readonly RoomQualityCreateValidator _createValidator;

    public RoomQualityCreateValidatorTests()
    {
        _createValidator = new RoomQualityCreateValidator();
    }

    [Fact]
    public async Task CreateValidate_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "Standard",
            SortOrder = 1,
            Description = "Standard quality room"
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
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "",
            SortOrder = 1,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.EMPTY_NAME);
    }

    [Fact]
    public async Task CreateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = new string('A', 51),
            SortOrder = 1,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.LONG_NAME);
    }

    [Fact]
    public async Task CreateValidate_SortOrder_BelowMinimum_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "Standard",
            SortOrder = -1,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.SortOrder)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.INVALID_SORT_ORDER);
    }

    [Fact]
    public async Task CreateValidate_SortOrder_AboveMaximum_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "Standard",
            SortOrder = 11,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.SortOrder)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.INVALID_SORT_ORDER);
    }

    [Fact]
    public async Task CreateValidate_SortOrder_AtBoundary_Zero_ReturnsSuccess()
    {
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "Standard",
            SortOrder = 0,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
    }

    [Fact]
    public async Task CreateValidate_SortOrder_AtBoundary_Ten_ReturnsSuccess()
    {
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "Standard",
            SortOrder = 10,
            Description = "Description"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SortOrder);
    }

    [Fact]
    public async Task CreateValidate_LongDescription_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "Standard",
            SortOrder = 1,
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
        // 1. Arrange
        var createDTO = new RoomQualityCreateDTO
        {
            Name = "Standard",
            SortOrder = 1,
            Description = null
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}

public class RoomQualityUpdateValidatorTests
{
    private readonly RoomQualityUpdateValidator _updateValidator;

    public RoomQualityUpdateValidatorTests()
    {
        _updateValidator = new RoomQualityUpdateValidator();
    }

    [Fact]
    public async Task UpdateValidate_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var updateDTO = new RoomQualityUpdateDTO
        {
            Name = "Deluxe",
            SortOrder = 3,
            Description = "Deluxe quality room"
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
        var updateDTO = new RoomQualityUpdateDTO
        {
            Name = "",
            SortOrder = 1,
            Description = "Description"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.EMPTY_NAME);
    }

    [Fact]
    public async Task UpdateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new RoomQualityUpdateDTO
        {
            Name = new string('A', 51),
            SortOrder = 1,
            Description = "Description"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.LONG_NAME);
    }

    [Fact]
    public async Task UpdateValidate_SortOrder_BelowMinimum_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new RoomQualityUpdateDTO
        {
            Name = "Deluxe",
            SortOrder = -1,
            Description = "Description"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.SortOrder)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.INVALID_SORT_ORDER);
    }

    [Fact]
    public async Task UpdateValidate_SortOrder_AboveMaximum_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new RoomQualityUpdateDTO
        {
            Name = "Deluxe",
            SortOrder = 11,
            Description = "Description"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.SortOrder)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomQuality.INVALID_SORT_ORDER);
    }

    [Fact]
    public async Task UpdateValidate_LongDescription_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new RoomQualityUpdateDTO
        {
            Name = "Deluxe",
            SortOrder = 1,
            Description = new string('A', 501)
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}
