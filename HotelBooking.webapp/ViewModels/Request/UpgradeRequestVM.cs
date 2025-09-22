public class UpgradeRequestVM
{
    public int RequestId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Address { get; set; } = "";
    public string TaxCode { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public DateTime RequestedAt { get; set; }
}
