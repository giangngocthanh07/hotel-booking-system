namespace HotelBooking.application.DTOs.Request.Overview
{
    /// <summary>
    /// Thống kê tổng quan cho Admin Dashboard
    /// </summary>
    public class RequestStatsDTO
    {
        public RequestTypeStatsDTO UpgradeRequest { get; set; } = new();
        public RequestTypeStatsDTO? HotelApproval { get; set; }  // Sau này

        // Tổng hợp
        public int TotalPending { get; set; }
        public int TotalToday { get; set; }
    }
}