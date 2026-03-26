using FluentAssertions;
using FluentValidation.TestHelper;
using HotelBooking.application.DTOs.Hotel;

public class RoomNameSuggestionValidatorTests
{
    private readonly RoomNameSuggestionValidator _validator;

    public RoomNameSuggestionValidatorTests()
    {
        _validator = new RoomNameSuggestionValidator();
    }

    [Fact]
    public void RoomNameSuggestionValidator_ValidRequest_ShouldPassValidation()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidUnitTypeId_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 0, // Invalid
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.UnitTypeId)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_UNIT_TYPE_ID_INVALID);
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidQualityId_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = -1, // Invalid
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.QualityId)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_QUALITY_ID_INVALID);
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidRoomViewId_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = -1, // Invalid
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.RoomViewId)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_ROOM_VIEW_ID_INVALID);
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidAdultCapacity_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 0, // Invalid
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.AdultCapacity)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_ADULT_CAPACITY_INVALID);
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidChildrenCapacity_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = -1, // Invalid
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 2 }
            }
        };
        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.ChildrenCapacity)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_CHILDREN_CAPACITY_REQUIRED);
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidMaxExtraBeds_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 0, // Invalid
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 2 }
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.MaxExtraBeds)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_MAX_EXTRA_BEDS_INVALID);
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidBedTypes_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = -1, Quantity = 2 } // Invalid
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.BedTypes[0].BedTypeId)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_BED_TYPES_INVALID);
    }

    [Fact]
    public void RoomNameSuggestionValidator_InvalidBedTypeQuantity_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>
            {
                new BedTypeConfigDTO { BedTypeId = 1, Quantity = 0 } // Invalid
            }
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.BedTypes[0].Quantity)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_BED_TYPES_QUANTITY_INVALID);
    }

    [Fact]
    public void RoomNameSuggestionValidator_MissingBedTypes_ShouldHaveValidationError()
    {
        // Arrange
        var request = new RoomNameSuggestionRequest
        {
            UnitTypeId = 1,
            QualityId = 2,
            RoomViewId = 3,
            AdultCapacity = 2,
            ChildrenCapacity = 1,
            CanAddExtraBeds = true,
            MaxExtraBeds = 1,
            BedTypes = new List<BedTypeConfigDTO>() // Empty
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert   
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(r => r.BedTypes)
            .WithErrorMessage(MessageResponse.RoomManagement.ROOM_NAME_SUGGESTION_BED_TYPES_REQUIRED);
    }
}
