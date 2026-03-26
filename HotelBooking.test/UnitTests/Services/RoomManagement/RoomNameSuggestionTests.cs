using FluentAssertions;
using FluentValidation;
using HotelBooking.application.DTOs.Hotel;
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
            var request = CreateRequest();

            // Setup mock attribute names based on the request
            SetupMockFacade();

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content;

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

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
            var request = CreateRequest();

            // Setup mock attribute names based on the request
            SetupMockFacade();

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content;

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

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
            var request = CreateRequest();

            // Setup mock attribute names based on the request
            SetupMockFacade();

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content;

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

            suggestedNames.Should().NotBeNullOrEmpty();

            // Template F: [UnitType] + [View]
            suggestedNames.Should().Contain("Suite with Sea View");

            // Template G: [Quality] + [UnitType] + [View]
            suggestedNames.Should().Contain("Deluxe Suite with Sea View");

            // Template H: [Quality] + [Bed] + [UnitType] + [View]
            suggestedNames.Should().Contain("Deluxe King Suite with Sea View");

            // Template I: [Bed] + [UnitType] + [View]
            suggestedNames.Should().Contain("King Suite with Sea View");
        }

        [Fact]
        public async Task SuggestRoomNamesAsync_Group4_FeatureFocus_ShouldReturnTemplatesJKLM_Multiplied()
        {
            // Mock validation to succeed
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RoomNameSuggestionRequest>(), default))
                .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            // 1. Arrange - Enable 2 features: Balcony and Private Bathroom
            var request = CreateRequest(hasBalcony: true, isPrivateBathroom: true);

            // Setup mock attribute names based on the request
            SetupMockFacade();

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content ?? new List<string>();

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

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
            var request = CreateRequest();

            // Setup mock attribute names based on the request
            SetupMockFacade();

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content ?? new List<string>();

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

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
            var request = CreateRequest(hasBalcony: true);

            // Setup mock attribute names based on the request
            SetupMockFacade();

            // 2. Act
            var result = await _service.SuggestRoomNamesAsync(request);
            var suggestedNames = result.Content ?? new List<string>();

            // 3. Assert
            result.Should().NotBeNull();
            result.Message.Should().Be(MessageResponse.Common.GET_SUCCESSFULLY);
            result.StatusCode.Should().Be(StatusCodeResponse.Success);

            // Template R: [UnitType] + [View] + [Feature]
            suggestedNames.Should().Contain("Suite with Sea View and Balcony");

            // Template S: [Quality] + [Bed] + [UnitType] + [View]
            suggestedNames.Should().Contain("Deluxe King Suite with Sea View");

            // Template T: [Quality] + [Bed] + [UnitType] + [Feature]
            suggestedNames.Should().Contain("Deluxe King Suite with Balcony");

            // Template U: [Quality] + [Bed] + [UnitType] + [View] + [Feature]
            suggestedNames.Should().Contain("Deluxe King Suite with Sea View and Balcony");
        }

        //***** HELPER METHODS *****//
        /// <summary>
        /// METHOD CREATE REQUEST
        /// </summary>
        /// <param name="hasBalcony"></param>
        /// <param name="hasTerrace"></param>
        /// <param name="isPrivateBathroom"></param>
        /// <param name="adultCapacity"></param>
        /// <returns></returns>
        private RoomNameSuggestionRequest CreateRequest(
            bool hasBalcony = false,
            bool hasTerrace = false,
            bool isPrivateBathroom = false,
            int adultCapacity = 2)
        {
            return new RoomNameSuggestionRequest
            {
                UnitTypeId = 1,
                QualityId = 2,
                RoomViewId = 3,
                HasBalcony = hasBalcony,
                HasTerrace = hasTerrace,
                IsPrivateBathroom = isPrivateBathroom,
                AdultCapacity = adultCapacity,
                ChildrenCapacity = 0,
                BedTypes = new List<BedTypeConfigDTO> { new() { BedTypeId = 1, Quantity = 1 } }
            };
        }

        /// <summary>
        /// SETUP MOCK FACADE METHOD
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="quality"></param>
        /// <param name="view"></param>
        /// <param name="bed"></param>
        private void SetupMockFacade(string unit = "Suite", string quality = "Deluxe", string view = "Sea View", string bed = "King")
        {
            var mockNamesDTO = new RoomAttributeNamesDTO
            {
                UnitTypeName = unit,
                QualityName = quality,
                RoomViewName = view,
                BedTypeNames = new List<BedTypeNameDTO> { new() { Name = bed, Quantity = 1 } }
            };

            _mockAttributeFacade.Setup(f => f.GetRoomAttributeNamesAsync(It.IsAny<RoomNameSuggestionRequest>()))
                .ReturnsAsync(mockNamesDTO);
        }
    }
}



