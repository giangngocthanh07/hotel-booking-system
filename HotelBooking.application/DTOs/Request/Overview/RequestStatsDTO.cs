namespace HotelBooking.application.DTOs.Request.Overview
{
    /// <summary>
    /// Statistics for Admin Dashboard
    /// </summary>
    public class RequestStatsDTO
    {
        public RequestTypeStatsDTO UpgradeRequest { get; set; } = new();
        public RequestTypeStatsDTO? HotelApproval { get; set; }  

        public int TotalPending { get; set; }
        public int TotalToday { get; set; }
    }
}