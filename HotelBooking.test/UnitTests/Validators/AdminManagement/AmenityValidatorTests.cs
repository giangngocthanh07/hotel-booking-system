
using FluentValidation.TestHelper;
using HotelBooking.application.Validators.AdminManagement.Amenities;
using HotelBooking.infrastructure.Models;

namespace HotelBooking.test.UnitTests.Validators.AdminManagement;

public class AmenityCreateValidatorTests
{
    AmenityCreateValidator _createValidator;
    public AmenityCreateValidatorTests()
    {
        _createValidator = new AmenityCreateValidator();
    }

    [Fact]
    public async Task CreateValidate_ValidRequest_ReturnsSuccess()
    {
        var createDTO = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = 1,
            Description = "Description 1"
        };

        var result = await _createValidator.TestValidateAsync(createDTO);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task CreateValidate_EmptyName_ReturnsBadRequest()
    {
        var createDTO = new AmenityCreateDTO
        {
            Name = "",
            TypeId = 1,
            Description = "Description 1"
        };

        var result = await _createValidator.TestValidateAsync(createDTO);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Amenity.EMPTY_NAME);
    }

    [Fact]
    public async Task CreateValidate_LongName_ReturnsBadRequest()
    {
        var createDTO = new AmenityCreateDTO
        {
            Name = new string('A', 51),
            TypeId = 1,
            Description = "Description 1"
        };

        var result = await _createValidator.TestValidateAsync(createDTO);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage(MessageResponse.AdminManagement.Amenity.LONG_NAME);
    }

    [Fact]
    public async Task CreateValidate_EmptyOrZeroTypeId_ReturnsBadRequest()
    {
        var createDTO = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = 0,
            Description = "Description 1"
        };

        var result = await _createValidator.TestValidateAsync(createDTO);
        result.ShouldHaveValidationErrorFor(x => x.TypeId)
            .WithErrorMessage(MessageResponse.AdminManagement.Amenity.EMPTY_TYPE_OR_GREATER_THAN_ZERO);
    }

    [Fact]
    public async Task CreateValidate_InvalidTypeId_ReturnsBadRequest()
    {
        var createDTO = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = -1,
            Description = "Description 1"
        };

        var result = await _createValidator.TestValidateAsync(createDTO);
        result.ShouldHaveValidationErrorFor(x => x.TypeId)
            .WithErrorMessage(MessageResponse.AdminManagement.Amenity.INVALID_TYPE);
    }

    [Fact]
    public async Task CreateValidate_LongDescription_ReturnsSuccess()
    {
        var createDTO = new AmenityCreateDTO
        {
            Name = "Amenity 1",
            TypeId = 1,
            Description = new string('A', 501)
        };

        var result = await _createValidator.TestValidateAsync(createDTO);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
    }


    public class AmenityUpdateValidatorTests
    {
        AmenityUpdateValidator _updateValidator;
        public AmenityUpdateValidatorTests()
        {
            _updateValidator = new AmenityUpdateValidator();
        }

        [Fact]
        public async Task UpdateValidate_ValidRequest_ReturnsSuccess()
        {
            var updateDTO = new AmenityUpdateDTO
            {
                Name = "Amenity 1",
                Description = "Description 1"
            };

            var result = await _updateValidator.TestValidateAsync(updateDTO);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task UpdateValidate_EmptyName_ReturnsBadRequest()
        {
            var updateDTO = new AmenityUpdateDTO
            {
                Name = "",
                Description = "Description 1"
            };

            var result = await _updateValidator.TestValidateAsync(updateDTO);
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(MessageResponse.AdminManagement.Amenity.EMPTY_NAME);
        }

        [Fact]
        public async Task UpdateValidate_LongName_ReturnsSuccess()
        {
            var updateDTO = new AmenityUpdateDTO
            {
                Name = new string('A', 51),
                Description = "Description 1"
            };

            var result = await _updateValidator.TestValidateAsync(updateDTO);
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(MessageResponse.AdminManagement.Amenity.LONG_NAME);
        }

        [Fact]
        public async Task UpdateValidate_LongDescription_ReturnsSuccess()
        {
            var updateDTO = new AmenityUpdateDTO
            {
                Name = "Amenity 1",
                Description = new string('A', 501)
            };

            var result = await _updateValidator.TestValidateAsync(updateDTO);
            result.ShouldHaveValidationErrorFor(x => x.Description)
                .WithErrorMessage(MessageResponse.Validation.LONG_DESCRIPTION);
        }
    }
}



