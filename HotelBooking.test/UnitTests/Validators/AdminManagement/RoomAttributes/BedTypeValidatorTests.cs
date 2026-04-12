
using FluentValidation.TestHelper;
using HotelBooking.application.Validators.AdminManagement.RoomAttributes;

namespace HotelBooking.test.UnitTests.Validators.AdminManagement.RoomAttributes;

public class BedTypeCreateValidatorTests
{
    BedTypeCreateValidator _createValidator;
    public BedTypeCreateValidatorTests()
    {
        _createValidator = new BedTypeCreateValidator();
    }

    [Fact]
    public async Task CreateValidate_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
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
        var createDTO = new BedTypeCreateDTO
        {
            Name = "",
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME);
    }

    [Fact]
    public async Task CreateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = new string('A', 101),
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.LONG_NAME);
    }

    [Fact]
    public async Task CreateValidate_InvalidDefaultCapacity_LessThanOne_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 0,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultCapacity)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);
    }

    [Fact]
    public async Task CreateValidate_InvalidDefaultCapacity_GreatherThanTen_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 11,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultCapacity)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);
    }

    // IsVaryingSize
    [Fact]
    public async Task CreateValidate_IsVaryingSizeIsTrue_IgnoresWidthValidation_ReturnsSuccess()
    {
        // Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = "Varying Bed",
            DefaultCapacity = 2,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = -5
        };

        // Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MinWidth);
        result.ShouldNotHaveValidationErrorFor(x => x.MaxWidth);
    }

    [Fact]
    public async Task CreateValidate_IsVaryingSizeIsFalse_InvalidMinWidth_ReturnsBadRequest()
    {
        // Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = "Standard Bed",
            DefaultCapacity = 2,
            IsVaryingSize = false,
            MinWidth = 0,
            MaxWidth = 100
        };

        // Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinWidth)
              .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MIN_WIDTH);
    }

    [Fact]
    public async Task CreateValidate_IsVaryingSizeIsFalse_MaxWidthLessThanMinWidth_ReturnsBadRequest()
    {
        // Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = "Standard Bed",
            DefaultCapacity = 2,
            IsVaryingSize = false,
            MinWidth = 100,
            MaxWidth = 50
        };

        // Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxWidth)
              .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MAX_WIDTH);
    }

    [Fact]
    public async Task CreateValidate_LongDescription_ReturnsBadRequest()
    {
        // 1. Arrange
        var createDTO = new BedTypeCreateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = new string('A', 1001)
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}

public class BedTypeUpdateValidatorTests
{
    BedTypeUpdateValidator _updateValidator;
    public BedTypeUpdateValidatorTests()
    {
        _updateValidator = new BedTypeUpdateValidator();
    }

    [Fact]
    public async Task UpdateValidate_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
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
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "",
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.EMPTY_NAME);
    }

    [Fact]
    public async Task UpdateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = new string('A', 101),
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.LONG_NAME);
    }


    [Fact]
    public async Task UpdateValidate_InvalidDefaultCapacity_LessThanOne_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 0,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultCapacity)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);
    }

    [Fact]
    public async Task UpdateValidate_InvalidDefaultCapacity_GreaterThanTen_ReturnsBadRequest()
    {
        // 1. Act
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 11,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.DefaultCapacity)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_DEFAULT_CAPACITY);
    }

    // IsVaryingSize
    [Fact]
    public async Task UpdateValidate_IsVaryingSizeIsTrue_IgnoresWidthValidation_ReturnsSuccess()
    {
        // 1. Arrange
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Varying Bed",
            DefaultCapacity = 2,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = -5
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MinWidth);
        result.ShouldNotHaveValidationErrorFor(x => x.MaxWidth);
    }

    [Fact]
    public async Task UpdateValidate_IsVaryingSizeIsFalse_InvalidMinWidth_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 1,
            IsVaryingSize = false,
            MinWidth = 0,
            MaxWidth = 10,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MinWidth)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MIN_WIDTH);
    }

    [Fact]
    public async Task UpdateValidate_IsVaryingSizeIsFalse_InvalidMaxWidth_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 1,
            IsVaryingSize = false,
            MinWidth = 10,
            MaxWidth = 5,
            Description = "Description 1"
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxWidth)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.BedType.INVALID_MAX_WIDTH);
    }

    [Fact]
    public async Task UpdateValidate_LongDescription_ReturnsBadRequest()
    {
        // 1. Arrange
        var updateDTO = new BedTypeUpdateDTO
        {
            Name = "Bed Type 1",
            DefaultCapacity = 1,
            IsVaryingSize = true,
            MinWidth = 0,
            MaxWidth = 0,
            Description = new string('A', 1001)
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }
}
