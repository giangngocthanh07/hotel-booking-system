
using FluentAssertions;
using HotelBooking.application.DTOs.Hotel;
using HotelBooking.application.Services.Domains.RoomManagement;
using Moq;

namespace HotelBooking.Tests.Services.RoomManagement
{
    public class RoomNameSuggestionTest : BaseServiceTest
    {
        private readonly Mock<IRoomAttributeFacade> _mockAttributeFacade;
        private readonly RoomNameSuggestionValidator _validator;
        private readonly RoomNameSuggestionService _service;

        public RoomNameSuggestionTest()
        {
            _mockAttributeFacade = new Mock<IRoomAttributeFacade>();
            _validator = new RoomNameSuggestionValidator();
            _service = new RoomNameSuggestionService(_mockAttributeFacade.Object, _validator);
        }

        #region Test Cases
        // 1. Failed Test Cases
        // ---------- a) The Null or Empty Request ----------
        [Fact]
        public async Task SuggestRoomNamesAsync_NullRequest_ReturnsBadRequest()
        {
            // Arrange
            RoomNameSuggestionRequest nullRequest = null!;

            // Act
            var result = await _service.SuggestRoomNamesAsync(nullRequest);

            // Assert
            result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
            result.Content.Should().BeNull();
        }

        [Fact]
        public async Task SuggestionRoomNamesAsync_RequiredFieldsAreNull_ReturnsBadRequest()
        {
            // Arrange
            var request = new RoomNameSuggestionRequest();

            // Act
            var result = await _validator.ValidateAsync(request);

            // Assert
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RoomNameSuggestionRequest.UnitTypeId));
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RoomNameSuggestionRequest.AdultCapacity));
        }

        // 2. Success Test Cases
        #endregion
    }
}