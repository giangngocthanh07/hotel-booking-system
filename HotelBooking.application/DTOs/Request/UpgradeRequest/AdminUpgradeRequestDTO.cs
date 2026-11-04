namespace HotelBooking.application.DTOs.Request.UpgradeRequest;

// FR-ONB-008: this Admin-only projection exposes an internal decision reason
// without leaking it through the Customer upgrade-request endpoints.
public class AdminUpgradeRequestDTO : UpgradeRequestDTO
{
    public string? RejectionReason { get; set; }
}
