namespace HotelBooking.webapp.ViewModels
{
    public class RegisterResponseVM
    {
        public bool IsSuccess { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}