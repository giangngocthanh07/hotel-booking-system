using System.ComponentModel.DataAnnotations;

public class CreateUpgradeRequestVM
{
    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã số thuế là bắt buộc")]
    public string TaxCode { get; set; } = string.Empty;
}