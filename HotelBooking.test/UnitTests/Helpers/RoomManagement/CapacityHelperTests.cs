using FluentAssertions;

public class CapacityHelperTests
{
    [Theory]
    // Only Adults
    [InlineData(1, 0, "with 1 Adult")]
    [InlineData(3, 0, "with 3 Adults")]
    // Children and Adults
    [InlineData(1, 1, "with 1 Adult and 1 Child")]
    [InlineData(1, 3, "with 1 Adult and 3 Children")]
    [InlineData(2, 1, "with 2 Adults and 1 Child")]
    [InlineData(2, 2, "with 2 Adults and 2 Children")]
    public void FormatCapacity_WhenAdultsArePresent_ShouldReturnSuccessWithData(int adults, int children, string expectedData)
    {
        // Act
        var result = CapacityHelper.FormatCapacity(adults, children);

        // Assert
        result.IsValid.Should().BeTrue();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Data.Should().Be(expectedData);
        result.Message.Should().BeNullOrEmpty();
    }

    [Theory]
    // Only Children
    [InlineData(0, 1, "1 Child")]
    [InlineData(0, 3, "3 Children")]
    public void FormatCapacity_WhenOnlyChildren_ShouldReturnSuccessWithData(int adults, int children, string expectedData)
    {
        // Act
        var result = CapacityHelper.FormatCapacity(adults, children);

        // Assert
        result.IsValid.Should().BeTrue();
        result.StatusCode.Should().Be(StatusCodeResponse.Success);
        result.Data.Should().Be(expectedData);
        result.Message.Should().BeNullOrEmpty();
    }

    [Theory]
    // Invalid Cases
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(-5, -2)]
    public void FormatCapacity_WhenInputsAreInvalid_ShouldReturnBadRequest(int adults, int children)
    {
        // Act
        var result = CapacityHelper.FormatCapacity(adults, children);

        // Assert
        result.IsValid.Should().BeFalse();
        result.StatusCode.Should().Be(StatusCodeResponse.BadRequest);
        result.Data.Should().BeNull();
        result.Message.Should().Be(MessageResponse.RoomManagement.ROOM_TYPE_CAPACITY_INVALID);
    }
}