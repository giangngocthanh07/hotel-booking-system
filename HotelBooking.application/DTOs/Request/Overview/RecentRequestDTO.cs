namespace HotelBooking.application.DTOs.Request.Overview
{
    /// <summary>
    /// Recent requests - used for dashboard widget
    /// </summary>
    public class RecentRequestDTO
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;        // "UpgradeOwner", "HotelApproval"
        public string TypeDisplay { get; set; } = string.Empty; // "Upgrade Owner", "Hotel Approval"
        public string RequesterName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}