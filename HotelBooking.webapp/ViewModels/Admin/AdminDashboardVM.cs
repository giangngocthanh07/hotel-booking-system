namespace HotelBooking.webapp.ViewModels.Admin;

public class AdminDashboardVM
{
    public decimal TotalRevenue { get; set; }
    public int TotalUsers { get; set; }
    public int TotalHotels { get; set; }
    public int TotalBookings { get; set; }
    
    public List<RecentRequestSummaryVM> PendingHotelRequests { get; set; } = new();
    public List<RecentRequestSummaryVM> PendingUpgradeRequests { get; set; } = new();
    public List<RevenueTrendVM> MonthlyRevenueTrend { get; set; } = new();
}

public class RecentRequestSummaryVM
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class RevenueTrendVM
{
    public string MonthName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
