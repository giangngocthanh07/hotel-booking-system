using FluentAssertions;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Validators.RoomManagement;

namespace HotelBooking.test.UnitTests.Validators.RoomManagement
{
    public class RoomTypeValidatorTests
    {
        private readonly RoomTypeCreateValidator _validator;

        public RoomTypeValidatorTests()
        {
            _validator = new RoomTypeCreateValidator();
        }

        [Fact]
        public void RoomTypeCreateValidator_ValidRequest_ShouldPassValidation()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                Description = "A spacious room with a king-size bed.",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                ChildCapacity = 1,
                UnitTypeId = 1,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = true,
                HasTerrace = false,
                CanAddExtraBed = true,
                MaxExtraBeds = 1,
                AreaSqm = 30.5f,
                IsSmokingAllowed = false,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 },
                    new BedTypeConfigDTO { BedTypeId = 2, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeTrue();

        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidHotelId_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 0,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "HotelId" && e.ErrorMessage == MessageResponse.RoomManagement.ROOM_TYPE_HOTEL_ID_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidUnitTypeId_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 0,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "UnitTypeId" && e.ErrorMessage == MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_ID_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidBedTypeId_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                CanAddExtraBed = true,
                MaxExtraBeds = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 0, Quantity = 1 } // Invalid BedTypeId
                }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "BedTypes[0].BedTypeId" && e.ErrorMessage == MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_ID_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidRoomViewId_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                RoomViewId = 0, // Invalid RoomViewId
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "RoomViewId" && e.ErrorMessage == MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_ID_INVALID);

        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidQualityId_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                QualityId = 0, // Invalid QualityId
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.Validate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == "QualityId" && e.ErrorMessage == MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_ID_INVALID);
        }
    }
}