using FluentValidation.TestHelper;
using HotelBooking.application.Validators.AdminManagement.RoomAttributes;

namespace HotelBooking.test.UnitTests.Validators.AdminManagement.RoomAttributes;

public class RoomViewCreateValidatorTests
{
    private readonly RoomViewCreateValidator _createValidator;

    public RoomViewCreateValidatorTests()
    {
        _createValidator = new RoomViewCreateValidator();
    }

    [Fact]
    public async Task CreateValidate_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var createDTO = new RoomViewCreateDTO
        {
            Name = "Sea View"
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
        var createDTO = new RoomViewCreateDTO
        {
            Name = ""
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomView.EMPTY_NAME);
    }

    [Fact]
    public async Task CreateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange — name exceeds MaximumLength(20)
        var createDTO = new RoomViewCreateDTO
        {
            Name = new string('A', 21)
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomView.LONG_NAME);
    }

    [Fact]
    public async Task CreateValidate_Name_AtMaxLength_ReturnsSuccess()
    {
        // 1. Arrange — exactly 20 chars (boundary)
        var createDTO = new RoomViewCreateDTO
        {
            Name = new string('A', 20)
        };

        // 2. Act
        var result = await _createValidator.TestValidateAsync(createDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}

public class RoomViewUpdateValidatorTests
{
    private readonly RoomViewUpdateValidator _updateValidator;

    public RoomViewUpdateValidatorTests()
    {
        _updateValidator = new RoomViewUpdateValidator();
    }

    [Fact]
    public async Task UpdateValidate_ValidRequest_ReturnsSuccess()
    {
        // 1. Arrange
        var updateDTO = new RoomViewUpdateDTO
        {
            Name = "Garden View"
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
        var updateDTO = new RoomViewUpdateDTO
        {
            Name = ""
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomView.EMPTY_NAME);
    }

    [Fact]
    public async Task UpdateValidate_LongName_ReturnsBadRequest()
    {
        // 1. Arrange — name exceeds MaximumLength(20)
        var updateDTO = new RoomViewUpdateDTO
        {
            Name = new string('A', 21)
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.RoomAttribute.RoomView.LONG_NAME);
    }

    [Fact]
    public async Task UpdateValidate_Name_AtMaxLength_ReturnsSuccess()
    {
        // 1. Arrange — exactly 20 chars (boundary)
        var updateDTO = new RoomViewUpdateDTO
        {
            Name = new string('A', 20)
        };

        // 2. Act
        var result = await _updateValidator.TestValidateAsync(updateDTO);

        // 3. Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
