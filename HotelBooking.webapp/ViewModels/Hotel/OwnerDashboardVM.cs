namespace HotelBooking.webapp.ViewModels.Hotel;

public class OwnerDashboardVM
{
    public int TodayArrivals { get; set; }
    public int TodayDepartures { get; set; }
    public int TotalStaying { get; set; }
    public decimal TotalRevenue { get; set; }
    public double OccupancyRate { get; set; }
    
    public List<OwnerRecentBookingVM> RecentBookings { get; set; } = new();
    public List<RoomAvailabilitySummaryVM> RoomAvailability { get; set; } = new();
    public List<DailyRevenueTrendVM> DailyRevenueTrend { get; set; } = new();
}

public class OwnerRecentBookingVM
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string RoomTypeName { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RoomAvailabilitySummaryVM
{
    public string RoomTypeName { get; set; } = string.Empty;
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
}

public class DailyRevenueTrendVM
{
    public string DateLabel { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
