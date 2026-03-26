using FluentAssertions;
using FluentValidation.TestHelper;
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
            var result = _validator.TestValidate(request);

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
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.HotelId)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_HOTEL_ID_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidPricePerNight_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = -10.00m, // Invalid price
                AdultCapacity = 2,
                UnitTypeId = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.PricePerNight)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_PRICE_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidAdultCapacity_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 0, // Invalid adult capacity
                UnitTypeId = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.AdultCapacity)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_ADULT_CAPACITY_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidChildCapacity_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                ChildCapacity = -1, // Invalid child capacity
                UnitTypeId = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.ChildCapacity)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_CHILD_CAPACITY_INVALID);
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
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.UnitTypeId)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_ID_INVALID);
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
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.QualityId)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_ID_INVALID);
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
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.RoomViewId)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_ID_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidMaxExtraBeds_ShouldHaveValidationError()
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
                MaxExtraBeds = 0, // Invalid MaxExtraBeds
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.MaxExtraBeds)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_MAX_EXTRA_BEDS_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_InvalidAreaSqm_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                AreaSqm = -5.0f, // Invalid area
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.AreaSqm)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_AREA_INVALID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RoomTypeCreateValidator_InvalidTotalRooms_ShouldHaveValidationError(int totalRooms)
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                TotalRooms = totalRooms, // Invalid total rooms
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new BedTypeConfigDTO { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.TotalRooms)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_TOTAL_ROOMS_INVALID);
        }

        [Fact]
        public void RoomTypeCreateValidator_MissingBedTypes_ShouldHaveValidationError()
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>() // Empty 
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeFalse();
            result.ShouldHaveValidationErrorFor(x => x.BedTypes)
                .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPES_REQUIRED);

        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RoomTypeCreateValidator_InvalidBedTypeId_ShouldHaveValidationError(int invalidBedTypeId)
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
        {
            new BedTypeConfigDTO { BedTypeId = invalidBedTypeId, Quantity = 1 }
        }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor("BedTypes[0].BedTypeId")
                  .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_ID_INVALID);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RoomTypeCreateValidator_InvalidBedTypeQuantity_ShouldHaveValidationError(int invalidQuantity)
        {
            // Arrange
            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Deluxe Room",
                PricePerNight = 150.00m,
                AdultCapacity = 2,
                UnitTypeId = 1,
                TotalRooms = 10,
                BedTypes = new List<BedTypeConfigDTO>
        {
            new BedTypeConfigDTO { BedTypeId = 1, Quantity = invalidQuantity }
        }
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor("BedTypes[0].Quantity")
                  .WithErrorMessage(MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_QUANTITY_INVALID);
        }
    }
}

