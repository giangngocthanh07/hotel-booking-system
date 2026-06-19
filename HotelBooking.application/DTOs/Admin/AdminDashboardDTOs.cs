namespace HotelBooking.application.DTOs.Admin;

public class AdminDashboardDTO
{
    public decimal TotalRevenue { get; set; }
    public int TotalUsers { get; set; }
    public int TotalHotels { get; set; }
    public int TotalBookings { get; set; }
    
    public List<RecentRequestSummaryDTO> PendingHotelRequests { get; set; } = new();
    public List<RecentRequestSummaryDTO> PendingUpgradeRequests { get; set; } = new();
    public List<RevenueTrendDTO> MonthlyRevenueTrend { get; set; } = new();
}

public class RecentRequestSummaryDTO
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RevenueTrendDTO
{
    public string MonthName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
