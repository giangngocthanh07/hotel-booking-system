namespace HotelBooking.application.DTOs.Request.HotelApproval;

// Rejection needs an explanation, so a body replaces the former ID-only command.
// Acting Admin identity remains a trusted claim and must not be supplied in this DTO.
public class RejectHotelRegistrationDTO
{
    public string Reason { get; set; } = string.Empty;
}
