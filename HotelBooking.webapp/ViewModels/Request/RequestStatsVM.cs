namespace HotelBooking.webapp.ViewModels.Request
{
    public class RequestStatsVM
    {
        public RequestTypeStatsVM UpgradeRequest { get; set; } = new();
        public RequestTypeStatsVM? HotelApproval { get; set; }
        public int TotalPending { get; set; }
        public int TotalToday { get; set; }
    }
}
