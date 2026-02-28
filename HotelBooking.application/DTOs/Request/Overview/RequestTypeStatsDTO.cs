namespace HotelBooking.application.DTOs.Request.Overview
{
    /// <summary>
    /// Stats cho từng loại request
    /// </summary>
    public class RequestTypeStatsDTO
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
        public int Today { get; set; }
        public int ThisWeek { get; set; }
        public int ThisMonth { get; set; }
    }
}