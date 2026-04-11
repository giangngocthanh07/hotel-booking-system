using HotelBooking.application.Services.Domains.RoomManagement;
using HotelBooking.application.DTOs.Hotel;

using Moq;
using FluentValidation;
using System.Linq.Expressions;
using HotelBooking.infrastructure.Models;
using FluentAssertions;

namespace HotelBooking.Tests.Services.RoomManagement
{
    public class RoomTypeServiceTests : BaseServiceTest
    {
        private readonly Mock<IHotelRepository> _mockHotelRepo;
        private readonly Mock<IRoomTypeRepository> _mockRoomTypeRepo;
        private readonly Mock<IRoomTypeBedConfigRepository> _mockBedConfigRepo;
        private readonly Mock<IRoomAttributeFacade> _mockAttributeFacade;
        private readonly Mock<IValidator<RoomTypeCreateDTO>> _mockValidator;
        private readonly IRoomTypeService _service;

        public RoomTypeServiceTests()
        {
            _mockHotelRepo = new Mock<IHotelRepository>();
            _mockRoomTypeRepo = new Mock<IRoomTypeRepository>();
            _mockBedConfigRepo = new Mock<IRoomTypeBedConfigRepository>();
            _mockAttributeFacade = new Mock<IRoomAttributeFacade>();
            _mockValidator = new Mock<IValidator<RoomTypeCreateDTO>>();
            _service = new RoomTypeService(_mockHotelRepo.Object, _mockRoomTypeRepo.Object, _mockBedConfigRepo.Object, _mockValidator.Object, _mockAttributeFacade.Object, _mockUnitOfWork.Object);
        }

        // --- TESTS FOR CreateRoomTypeAsync ---
        // Happy path: Valid request should create room type successfully
        [Fact]
        public async Task CreateRoomTypeAsync_ValidRequest_ShouldReturnSuccess()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist();

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                Description = "A test room type",
                IsDeleted = false,
                PricePerNight = 100,
                AdultCapacity = 2,
                ChildCapacity = 0,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBed = false,
                MaxExtraBeds = 0,
                AreaSqm = 20,
                IsSmokingAllowed = false,
                TotalRooms = 1,
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // 2. Act 
            var actualResult = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            actualResult.Should().NotBeNull();
            actualResult.StatusCode.Should().Be(StatusCodeResponse.Success);
            actualResult.Message.Should().Be(MessageResponse.Common.CREATE_SUCCESSFULLY);
            actualResult.Content.Should().BeEquivalentTo(request, options => options
        .ExcludingMissingMembers());

            // Verify that validation, hotel existence check, and attribute existence checks were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            Verify_Repo_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo, 1);
            Verify_Repo_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo, request.BedTypes.Count);
            Verify_Saved(2); // Once for RoomType and once for BedConfigs
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Once);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_WhenRequestIsNull_ShouldReturnBadRequest()
        {
            // 1. Arrange
            RoomTypeCreateDTO request = null!;

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
            result.Message.Should().Be(MessageResponse.Common.REQUEST_CANNOT_BE_NULL);
            result.Content.Should().BeNull(); // Default value for class when creation fails


            // Make sure that validation and existence checks were never called
            _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<RoomTypeCreateDTO>(), default), Times.Never);
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_InvalidRequest_ShouldReturnBadRequest()
        {
            // 1. Arrange
            var validationFailures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Name", MessageResponse.RoomManagement.ROOM_TYPE_NAME_EMPTY)
            };
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomTypeCreateDTO>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult(validationFailures));

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "", // Invalid name
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
            result.Message.Should().Be(validationFailures.First().ErrorMessage);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation was called but existence checks were not called
            _mockValidator.Verify(v => v.ValidateAsync(It.IsAny<RoomTypeCreateDTO>(), default), Times.Once);

            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_HotelNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist(hotelExists: false);

            var request = new RoomTypeCreateDTO
            {
                HotelId = 999, // Non-existent hotel ID
                Name = "Test Room Type",
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.RoomManagement.ROOM_TYPE_HOTEL_NOT_FOUND);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation and hotel existence check were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo);
            Verify_Never_Saved();
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_UnitTypeNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist(unitTypeExists: false);

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                UnitTypeId = 999, // Non-existent unit type ID
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.RoomManagement.ROOM_TYPE_UNIT_TYPE_NOT_FOUND);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation, hotel existence check, and unit type existence check were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo);
            Verify_Never_Saved();
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_QualityNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist(qualityExists: false);

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                UnitTypeId = 1,
                QualityId = 999, // Non-existent quality ID
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.RoomManagement.ROOM_TYPE_QUALITY_NOT_FOUND);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation, hotel existence check, unit type existence check, and quality existence check were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo);
            Verify_Never_Saved();
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_RoomViewNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist(roomViewExists: false);

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                UnitTypeId = 1,
                QualityId = 1,
                RoomViewId = 999, // Non-existent room view ID
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.RoomManagement.ROOM_TYPE_ROOM_VIEW_NOT_FOUND);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation, hotel existence check, unit type existence check, quality existence check, and room view existence check were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo);
            Verify_Never_Saved();
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_BedTypeNotFound_ShouldReturnNotFound()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist(bedTypeExists: false);

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                UnitTypeId = 1,
                QualityId = 1,
                RoomViewId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 999, Quantity = 1 } } // Non-existent bed type ID
            };

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.NotFound);
            result.Message.Should().Be(MessageResponse.RoomManagement.ROOM_TYPE_BED_TYPE_NOT_FOUND);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation, hotel existence check, unit type existence check, quality existence check, room view existence check, and bed type existence check were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Never);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo);
            Verify_Never_Saved();
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_RoomTypeAlreadyExists_ShouldReturnConflict()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist();

            // Mock RoomTypeAlreadyExist
            _mockRoomTypeRepo
                .Setup(rt => rt.AnyAsync(It.IsAny<Expression<Func<RoomType, bool>>>()))
                .ReturnsAsync(true);

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Room Type Already Exists",
                Description = "A test room type",
                IsDeleted = false,
                PricePerNight = 100,
                AdultCapacity = 2,
                ChildCapacity = 0,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBed = false,
                MaxExtraBeds = 0,
                AreaSqm = 20,
                IsSmokingAllowed = false,
                TotalRooms = 1,
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            });

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Conflict);
            result.Message.Should().Be(MessageResponse.RoomManagement.ROOM_TYPE_ALREADY_EXISTS);
            result.Content.Should().BeNull(); // Default value for class when creation fails)

            // Verify
            Verify_Repo_AnyAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRoomTypeAsync_SystemThrowExceptionAtAnyAsync_ShouldRollbackAndReturnError()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist();

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                Description = "A test room type",
                IsDeleted = false,
                PricePerNight = 100,
                AdultCapacity = 2,
                ChildCapacity = 0,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBed = false,
                MaxExtraBeds = 0,
                AreaSqm = 20,
                IsSmokingAllowed = false,
                TotalRooms = 1,
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            _mockRoomTypeRepo
                .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<RoomType, bool>>>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));


            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify
            Verify_Repo_AnyAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRoomTypeAsync_SystemThrowExceptionAtAddAsync_ShouldRollbackAndReturnError()
        {
            // 1. Arrange
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist();

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                Description = "A test room type",
                IsDeleted = false,
                PricePerNight = 100,
                AdultCapacity = 2,
                ChildCapacity = 0,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBed = false,
                MaxExtraBeds = 0,
                AreaSqm = 20,
                IsSmokingAllowed = false,
                TotalRooms = 1,
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // Mock not existing roomtype
            _mockRoomTypeRepo
                .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<RoomType, bool>>>()))
                .ReturnsAsync(false);

            // Mock AddAsync fail --> FAIL FAST
            _mockRoomTypeRepo
                .Setup(r => r.AddAsync(It.IsAny<RoomType>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify
            Verify_Repo_AnyAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo, 1);
            Verify_Never_Saved();
        }

        [Fact]
        public async Task CreateRoomTypeAsync_SystemThrowExceptionAtSaveChangesAsync_ShouldRollbackAndReturnError()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist();

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                Description = "A test room type",
                IsDeleted = false,
                PricePerNight = 100,
                AdultCapacity = 2,
                ChildCapacity = 0,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBed = false,
                MaxExtraBeds = 0,
                AreaSqm = 20,
                IsSmokingAllowed = false,
                TotalRooms = 1,
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // Simulate an exception when saving the RoomType (e.g., database error)
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation, hotel existence check, and attribute existence checks were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            Verify_Repo_AnyAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_Never_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo);
            Verify_Saved(1); // Only attempted to save the RoomType before the exception
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_SystemThrowExceptionAtAddAsyncBedConfig_ShouldRollbackAndReturnError()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist();

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                Description = "A test room type",
                IsDeleted = false,
                PricePerNight = 100,
                AdultCapacity = 2,
                ChildCapacity = 0,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBed = false,
                MaxExtraBeds = 0,
                AreaSqm = 20,
                IsSmokingAllowed = false,
                TotalRooms = 1,
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // Mock AddAsync BedType fail --> FAIL FAST
            _mockBedConfigRepo
                .Setup(r => r.AddAsync(It.IsAny<RoomTypeBedConfig>()))
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER));

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            result.Content.Should().BeNull();

            // Verify
            Verify_Repo_AnyAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Saved(1); // Only attempted to save the RoomType before the exception
            Verify_Repo_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo);
        }

        [Fact]
        public async Task CreateRoomTypeAsync_SystemThrowExceptionAtSaveBedConfig_ShouldRollbackAndReturnError()
        {
            // 1. Arrange
            MockValidationSuccess();
            MockAllGhostIdsExist();

            var request = new RoomTypeCreateDTO
            {
                HotelId = 1,
                Name = "Test Room Type",
                Description = "A test room type",
                IsDeleted = false,
                PricePerNight = 100,
                AdultCapacity = 2,
                ChildCapacity = 0,
                QualityId = 1,
                RoomViewId = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBed = false,
                MaxExtraBeds = 0,
                AreaSqm = 20,
                IsSmokingAllowed = false,
                TotalRooms = 1,
                UnitTypeId = 1,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            // Simulate an exception when saving the BedConfig (e.g., database error)
            _mockUnitOfWork.SetupSequence(u => u.SaveChangesAsync())
                .ReturnsAsync(1) // First save (RoomType) succeeds
                .ThrowsAsync(new Exception(MessageResponse.Common.ERROR_IN_SERVER)); // Second save (BedConfig) fails

            // 2. Act
            var result = await _service.CreateRoomTypeAsync(request);

            // 3. Assert
            result.StatusCode.Should().Be(StatusCodeResponse.Error);
            result.Message.Should().Be(MessageResponse.Common.ERROR_IN_SERVER);
            result.Content.Should().BeNull(); // Default value for class when creation fails

            // Verify that validation, hotel existence check, and attribute existence checks were called
            _mockUnitOfWork.Verify(u => u.BeginTransactionAsync(), Times.Once);
            Verify_Repo_AnyAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_AddAsync<IRoomTypeRepository, RoomType>(_mockRoomTypeRepo);
            Verify_Repo_AddAsync<IRoomTypeBedConfigRepository, RoomTypeBedConfig>(_mockBedConfigRepo, request.BedTypes.Count);
            Verify_Saved(2); // Attempted to save both RoomType and BedConfig before the exception
            _mockUnitOfWork.Verify(u => u.CommitTransactionAsync(), Times.Never);
            _mockUnitOfWork.Verify(u => u.RollBackTransactionAsync(), Times.Once);
        }


        // --- Helpers ---
        private void MockValidationSuccess()
        {
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomTypeCreateDTO>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        }

        private void MockAllGhostIdsExist(
        bool hotelExists = true,
        bool unitTypeExists = true,
        bool qualityExists = true,
        bool roomViewExists = true,
        bool bedTypeExists = true,
        bool roomTypeAlreadyExists = false)
        {
            _mockHotelRepo
                .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Hotel, bool>>>()))
                .ReturnsAsync(hotelExists);

            _mockAttributeFacade
                .Setup(f => f.IsUnitTypeExistedAsync(It.IsAny<int>()))
                .ReturnsAsync(unitTypeExists);

            _mockAttributeFacade
                .Setup(f => f.IsRoomQualityExistedAsync(It.IsAny<int>()))
                .ReturnsAsync(qualityExists);

            _mockAttributeFacade
                .Setup(f => f.IsRoomViewExistedAsync(It.IsAny<int>()))
                .ReturnsAsync(roomViewExists);

            _mockAttributeFacade
                .Setup(f => f.IsBedTypeExistedAsync(It.IsAny<int>()))
                .ReturnsAsync(bedTypeExists);

            _mockRoomTypeRepo
                .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<RoomType, bool>>>()))
                .ReturnsAsync(roomTypeAlreadyExists);
        }
    }
}
