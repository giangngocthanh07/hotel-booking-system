namespace HotelBooking.application.DTOs.Request.UpgradeRequest;

// FR-ONB-008: require an explanation without accepting a client-provided Admin identity.
public class RejectUpgradeRequestDTO
{
    public string Reason { get; set; } = string.Empty;
}
