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
    public void FormatCapacity_WhenAdultsArePresent_ShouldReturnCorrectFormat(int adults, int children, string expected)
    {
        // Act
        string result = CapacityHelper.FormatCapacity(adults, children);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, 2)]
    [InlineData(2, -1)]
    public void FormatCapacity_WhenInputsAreInvalid_ShouldThrowArgumentOutOfRangeException(int adults, int children)
    {
        // Act
        Action act = () => CapacityHelper.FormatCapacity(adults, children);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_CAPACITY_INVALID);
    }

    [Fact]
    public void FormatCapacity_WhenBothAreZero_ShouldThrowArgumentException()
    {
        // Arrange
        int adults = 0;
        int children = 0;

        // Act
        Action act = () => CapacityHelper.FormatCapacity(adults, children);

        // Assert
        act.Should().Throw<ArgumentException>(MessageResponse.RoomManagement.ROOM_TYPE_CAPACITY_INVALID)
           .WithMessage(MessageResponse.RoomManagement.ROOM_TYPE_CAPACITY_INVALID);
    }
}