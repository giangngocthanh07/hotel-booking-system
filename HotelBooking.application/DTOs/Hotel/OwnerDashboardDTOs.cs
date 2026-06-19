namespace HotelBooking.application.DTOs.Hotel;

public class OwnerDashboardDTO
{
    public int TodayArrivals { get; set; }
    public int TodayDepartures { get; set; }
    public int TotalStaying { get; set; }
    public decimal TotalRevenue { get; set; }
    public double OccupancyRate { get; set; }
    
    public List<OwnerRecentBookingDTO> RecentBookings { get; set; } = new();
    public List<RoomAvailabilitySummaryDTO> RoomAvailability { get; set; } = new();
    public List<RevenueTrendDTO> DailyRevenueTrend { get; set; } = new();
}

public class OwnerRecentBookingDTO
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RoomAvailabilitySummaryDTO
{
    public string RoomTypeName { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
}

public class RevenueTrendDTO
{
    public string DateLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
