namespace HotelBooking.webapp.ViewModels.Request
{
    /// <summary>
    /// ViewModel for displaying recent requests in the dashboard.
    /// </summary>
    public class RecentRequestVM
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string TypeDisplay { get; set; } = string.Empty;
        public string RequesterName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}