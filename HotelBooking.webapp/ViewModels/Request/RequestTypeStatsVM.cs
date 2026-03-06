namespace HotelBooking.webapp.ViewModels.Request
{
    /// <summary>
    /// ViewModel for displaying request type statistics in the dashboard.
    /// </summary>
    public class RequestTypeStatsVM
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
        public int Total { get; set; }
        public int Today { get; set; }
        public int ThisWeek { get; set; }
        public int ThisMonth { get; set; }
    }
}