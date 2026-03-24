
using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Helpers;
using HotelBooking.application.Services.Domains.RoomManagement;
using Moq;

namespace HotelBooking.Tests.Services.RoomManagement
{
    public class RoomNameSuggestionTests : BaseServiceTest
    {
        private readonly Mock<IRoomAttributeFacade> _mockAttributeFacade;
        private readonly Mock<IValidator<RoomNameSuggestionRequest>> _mockValidator;
        private readonly RoomNameSuggestionService _service;

        public RoomNameSuggestionTests()
        {
            _mockAttributeFacade = new Mock<IRoomAttributeFacade>();
            _mockValidator = new Mock<IValidator<RoomNameSuggestionRequest>>();
            _service = new RoomNameSuggestionService(_mockAttributeFacade.Object, _mockValidator.Object);
        }

        [Fact]
        public async Task SuggestRoomNamesAsync_Group1_QualityFocus_ShouldReturnTemplatesABC()
        {
            // Mock validation to succeed
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomNameSuggestionRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // 1. Arrange - Group 1: Focus on Quality
            var request = new RoomNameSuggestionRequest
            {
                UnitTypeId = 1, // Assume 1 corresponds to "Double Room"
                QualityId = 2, // Assume 2 corresponds to "Deluxe"
                RoomViewId = null,
                AdultCapacity = 2,
                ChildrenCapacity = 0,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBeds = false,
                MaxExtraBeds = null,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new() { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Setup mock attribute names based on the request
            var mockNamesDTO = new RoomAttributeNamesDTO
            {
                UnitTypeName = "Suite",
                QualityName = "Deluxe",
                BedTypeNames = new List<BedTypeNameDTO> { new() { Name = "King", Quantity = 1 } },
                RoomViewName = null
            };

            // Mock the facade to return expected attribute names
            _mockAttributeFacade.Setup(f => f.GetRoomAttributeNamesAsync(It.IsAny<RoomNameSuggestionRequest>()))
                .ReturnsAsync(mockNamesDTO);

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content;

            // 3. Assert
            result.Should().NotBeNull();
            suggestedNames.Should().NotBeNullOrEmpty();

            // Template A: [UnitType]
            suggestedNames.Should().Contain("Suite");

            // Template B: [Quality] + [UnitType]
            suggestedNames.Should().Contain("Deluxe Suite");

            // Template C: [BedType] + [UnitType]
            suggestedNames.Should().Contain("King Suite");

        }

        [Fact]
        public async Task SuggestRoomNamesAsync_Group2_BedAndQualityFocus_ShouldReturnTemplatesDE()
        {
            // Mock validation to succeed
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomNameSuggestionRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // 1. Arrange - Group 2: Focus on Bed Configuration and Quality
            var request = new RoomNameSuggestionRequest
            {
                UnitTypeId = 1, // Assume 1 corresponds to "Double Room"
                QualityId = 2, // Assume 2 corresponds to "Deluxe"
                RoomViewId = 1, // Assume 1 corresponds to "Sea View"
                AdultCapacity = 2,
                ChildrenCapacity = 0,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBeds = false,
                MaxExtraBeds = null,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new() { BedTypeId = 1, Quantity = 1 }
                }
            };

            // Setup mock attribute names based on the request
            var mockNamesDTO = new RoomAttributeNamesDTO
            {
                UnitTypeName = "Suite",
                QualityName = "Deluxe",
                BedTypeNames = new List<BedTypeNameDTO> { new() { Name = "King", Quantity = 1 } },
                RoomViewName = "Sea View"
            };

            // Mock the facade to return expected attribute names
            _mockAttributeFacade.Setup(f => f.GetRoomAttributeNamesAsync(It.IsAny<RoomNameSuggestionRequest>()))
                .ReturnsAsync(mockNamesDTO);

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content;

            // 3. Assert
            result.Should().NotBeNull();
            suggestedNames.Should().NotBeNullOrEmpty();

            // Template D: [Quality] + [BedType] + [UnitType]
            suggestedNames.Should().Contain("Deluxe King Suite");
            // Template E: [UnitType] + [View] + [Capacity]
            suggestedNames.Should().Contain("Suite with Sea View for 2 Adults");
        }

        [Fact]
        public async Task SuggestRoomNamesAsync_Group3_ViewFocus_ShouldReturnTemplateFGHI()
        {
            // Mock validation to succeed
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomNameSuggestionRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // 1. Arrange - Group 3: Focus on Room View and Capacity
            var request = new RoomNameSuggestionRequest
            {
                UnitTypeId = 1, // Assume 1 corresponds to "Double Room"
                QualityId = 2, // Assume 1 corresponds to "Standard"
                RoomViewId = 3, // Assume 1 corresponds to "Sea View"
                AdultCapacity = 2,
                ChildrenCapacity = 1,
                IsPrivateBathroom = true,
                HasBalcony = false,
                HasTerrace = false,
                CanAddExtraBeds = false,
                MaxExtraBeds = null,
                BedTypes = new List<BedTypeConfigDTO>
                {
                    new() { BedTypeId = 1, Quantity = 2 }
                }
            };

            // Setup mock attribute names based on the request
            var mockNamesDTO = new RoomAttributeNamesDTO
            {
                UnitTypeName = "Suite",
                QualityName = "Deluxe",
                BedTypeNames = new List<BedTypeNameDTO> { new() { Name = "King", Quantity = 2 } },
                RoomViewName = "Sea View"
            };

            // Mock the facade to return expected attribute names
            _mockAttributeFacade.Setup(f => f.GetRoomAttributeNamesAsync(It.IsAny<RoomNameSuggestionRequest>()))
                .ReturnsAsync(mockNamesDTO);

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content;

            // 3. Assert
            result.Should().NotBeNull();
            suggestedNames.Should().NotBeNullOrEmpty();

            // Template F: [UnitType] + [View]
            suggestedNames.Should().Contain("Suite with Sea View");

            // Template G: [Quality] + [UnitType] + [View]
            suggestedNames.Should().Contain("Deluxe Suite with Sea View");

            // Template H: [Quality] + [Bed] + [UnitType] + [View]
            suggestedNames.Should().Contain("Deluxe Twin King Suite with Sea View");

            // Template I: [Bed] + [UnitType] + [View]
            suggestedNames.Should().Contain("Twin King Suite with Sea View");
        }

        [Fact]
        public async Task SuggestRoomNamesAsync_Group4_FeatureFocus_ShouldReturnTemplatesJKLM_Multiplied()
        {
            // Mock validation to succeed
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomNameSuggestionRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // 1. Arrange - Enable 2 features: Balcony and Private Bathroom
            var request = new RoomNameSuggestionRequest
            {
                UnitTypeId = 1,
                QualityId = 2,
                RoomViewId = null,
                AdultCapacity = 2,
                ChildrenCapacity = 0,
                IsPrivateBathroom = true,
                HasBalcony = true,
                HasTerrace = false,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            var mockNamesDTO = new RoomAttributeNamesDTO
            {
                UnitTypeName = "Suite",
                QualityName = "Deluxe",
                BedTypeNames = new List<BedTypeNameDTO> { new() { Name = "King", Quantity = 1 } }
            };

            _mockAttributeFacade.Setup(f => f.GetRoomAttributeNamesAsync(It.IsAny<RoomNameSuggestionRequest>()))
                .ReturnsAsync(mockNamesDTO);

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content ?? new List<string>();

            // 3. Assert
            result.Should().NotBeNull();
            suggestedNames.Should().NotBeNullOrEmpty();

            // --- Check Feature: Balcony ---
            suggestedNames.Should().Contain("Suite with Balcony");               // J
            suggestedNames.Should().Contain("Deluxe Suite with Balcony");        // K
            suggestedNames.Should().Contain("King Suite with Balcony");          // L
            suggestedNames.Should().Contain("Deluxe King Suite with Balcony");   // M

            // --- Check Feature: Private Bathroom ---
            suggestedNames.Should().Contain("Suite with Private Bathroom");               // J
            suggestedNames.Should().Contain("Deluxe Suite with Private Bathroom");        // K
            suggestedNames.Should().Contain("King Suite with Private Bathroom");          // L
            suggestedNames.Should().Contain("Deluxe King Suite with Private Bathroom");   // M

            // Ensure no names contain "Terrace"
            suggestedNames.Should().NotContain(name => name.Contains("Terrace"));
        }

        [Fact]
        public async Task SuggestRoomNamesAsync_Group5_CapacityFocus_ShouldReturnTemplatesNOPQ()
        {
            // Mock validation to succeed
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomNameSuggestionRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // 1. Arrange - Focus vào Capacity
            var request = new RoomNameSuggestionRequest
            {
                UnitTypeId = 1,
                QualityId = 2,
                RoomViewId = null,
                AdultCapacity = 2,
                ChildrenCapacity = 0,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            var mockNamesDTO = new RoomAttributeNamesDTO
            {
                UnitTypeName = "Suite",
                QualityName = "Deluxe",
                BedTypeNames = new List<BedTypeNameDTO> { new() { Name = "King", Quantity = 1 } }
            };

            _mockAttributeFacade.Setup(f => f.GetRoomAttributeNamesAsync(It.IsAny<RoomNameSuggestionRequest>()))
                .ReturnsAsync(mockNamesDTO);

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content ?? new List<string>();

            // 3. Assert
            result.Should().NotBeNull();
            suggestedNames.Should().NotBeNullOrEmpty();

            // Template N: [UnitType] + [Capacity]
            suggestedNames.Should().Contain("Suite for 2 Adults");

            // Template O: [Quality] + [UnitType] + [Capacity]
            suggestedNames.Should().Contain("Deluxe Suite for 2 Adults");

            // Template P: [Bed] + [UnitType] + [Capacity]
            suggestedNames.Should().Contain("King Suite for 2 Adults");

            // Template Q: [Quality] + [Bed] + [UnitType] + [Capacity]
            suggestedNames.Should().Contain("Deluxe King Suite for 2 Adults");
        }

        [Fact]
        public async Task SuggestRoomNamesAsync_Group6_FullCombo_ShouldReturnTemplatesRSTU()
        {
            // Mock validation
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomNameSuggestionRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // 1. Arrange - Full combo
            var request = new RoomNameSuggestionRequest
            {
                UnitTypeId = 1,
                QualityId = 2,
                RoomViewId = 3,
                HasBalcony = true,        // Enable 1 feature to test the "and" conjunction
                IsPrivateBathroom = false,
                HasTerrace = false,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };

            var mockNamesDTO = new RoomAttributeNamesDTO
            {
                UnitTypeName = "Suite",
                QualityName = "Deluxe",
                RoomViewName = "Sea View",
                BedTypeNames = new List<BedTypeNameDTO> { new() { Name = "King", Quantity = 1 } }
            };

            _mockAttributeFacade.Setup(f => f.GetRoomAttributeNamesAsync(It.IsAny<RoomNameSuggestionRequest>()))
                .ReturnsAsync(mockNamesDTO);

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content ?? new List<string>();

            // 3. Assert
            result.Should().NotBeNull();

            // Template R: [UnitType] + [View] + [Feature]
            suggestedNames.Should().Contain("Suite with Sea View and Balcony");

            // Template S: [Quality] + [Bed] + [UnitType] + [View]
            suggestedNames.Should().Contain("Deluxe King Suite with Sea View");

            // Template T: [Quality] + [Bed] + [UnitType] + [Feature]
            suggestedNames.Should().Contain("Deluxe King Suite with Balcony");

            // Template U: [Quality] + [Bed] + [UnitType] + [View] + [Feature]
            suggestedNames.Should().Contain("Deluxe King Suite with Sea View and Balcony");
        }
    }
}
