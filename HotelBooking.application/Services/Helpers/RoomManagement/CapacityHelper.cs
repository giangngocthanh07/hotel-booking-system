public static class CapacityHelper
{
    public static ValidationResult<string> FormatCapacity(int adults, int children)
    {
        // 1. Guard Clauses: Validate inputs upfront to fail fast on invalid data
        if (adults < 0 || children < 0)
        {
            return ValidationResult<string>.Fail(MessageResponse.RoomManagement.ROOM_TYPE_CAPACITY_INVALID, StatusCodeResponse.BadRequest);

        }
        if (adults == 0 && children == 0)
        {
            return ValidationResult<string>.Fail(MessageResponse.RoomManagement.ROOM_TYPE_CAPACITY_INVALID, StatusCodeResponse.BadRequest);
        }

        const string WITH = "with";
        const string AND = "and";

        // 2. Using Switch Expression to handle the Children string more elegantly
        string childPart = children switch
        {
            0 => string.Empty,
            1 => "1 Child",
            _ => $"{children} Children"
        };

        // 3. Without Adults
        if (adults == 0)
        {
            return ValidationResult<string>.Success(childPart); // Return "1 Child" or "X Children" without "with"
        }

        // 4. With Adults
        string adultPart = adults >= 2 ? $"{adults} Adults" : "1 Adult";

        // 5. Merge Adults and Children parts
        if (string.IsNullOrEmpty(childPart))
        {
            return ValidationResult<string>.Success($"{WITH} {adultPart}");
        }

        return ValidationResult<string>.Success($"{WITH} {adultPart} {AND} {childPart}");
    }

}
