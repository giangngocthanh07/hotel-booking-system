namespace HotelBooking.webapp.ViewModels
{

    public class UserForUpgradeVM
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? RequestStatus { get; set; } = "None";
    }
}