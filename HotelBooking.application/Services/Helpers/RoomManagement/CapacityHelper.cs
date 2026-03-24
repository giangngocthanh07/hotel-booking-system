public static class CapacityHelper
{
    public static string FormatCapacity(int adults, int children)
    {
        // 1. Guard Clauses: Validate inputs upfront to fail fast on invalid data
        if (adults < 0 || children < 0)
        {
            return string.Empty;

        }
        if (adults == 0 && children == 0)
        {
            return string.Empty;
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
            return childPart; // Return "1 Child" or "X Children" without "with"
        }

        // 4. With Adults
        string adultPart = adults >= 2 ? $"{adults} Adults" : "1 Adult";

        // 5. Merge Adults and Children parts
        if (string.IsNullOrEmpty(childPart))
        {
            return $"{WITH} {adultPart}";
        }

        return $"{WITH} {adultPart} {AND} {childPart}";
    }

}
