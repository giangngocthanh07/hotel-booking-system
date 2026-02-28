namespace HotelBooking.application.DTOs.Request.Overview
{
    /// <summary>
    /// Request gần đây - dùng cho widget dashboard
    /// </summary>
    public class RecentRequestDTO
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;        // "UpgradeOwner", "HotelApproval"
        public string TypeDisplay { get; set; } = string.Empty; // "Nâng cấp Owner", "Duyệt khách sạn"
        public string RequesterName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}