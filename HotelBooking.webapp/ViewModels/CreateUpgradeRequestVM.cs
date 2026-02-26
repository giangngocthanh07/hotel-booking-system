using System.ComponentModel.DataAnnotations;

public class CreateUpgradeRequestVM
{
    [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
    public string Address { get; set; } = null!;

    [Required(ErrorMessage = "Mã số thuế là bắt buộc")]
    public string TaxCode { get; set; } = null!;
}