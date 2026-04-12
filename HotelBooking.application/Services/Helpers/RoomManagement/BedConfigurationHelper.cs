using HotelBooking.application.DTOs.Hotel;

public static class BedConfigurationHelper
{
    public static string FormatBedConfiguration(IEnumerable<BedTypeNameDTO> bedTypes)
    {
        if (bedTypes == null)
        {
            return string.Empty;
        }

        if (!bedTypes.Any())
        {
            return string.Empty;
        }

        var bedDescriptions = bedTypes.Select(bedType =>
        {
            string bedName = bedType.Name.Trim(); // Trim for safety
            if (bedName.Equals("Single", StringComparison.OrdinalIgnoreCase))
            {
                return bedType.Quantity switch
                {
                    1 => "Single",
                    2 => "Twin",
                    3 => "Triple",
                    4 => "Quadruple",
                    _ => $"{bedType.Quantity} Singles"
                };
            }
            return bedType.Quantity switch
            {
                1 => bedName,
                2 => $"Twin {bedName}",
                3 => $"Triple {bedName}",
                4 => $"Quadruple {bedName}",
                _ => $"{bedType.Quantity} {bedName}s"
            };
        });

        var validDescriptions = bedDescriptions.Where(d => !string.IsNullOrWhiteSpace(d));
        return string.Join(" and ", validDescriptions);
    }
}