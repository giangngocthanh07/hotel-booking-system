
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
    }
}
