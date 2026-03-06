using System.ComponentModel.DataAnnotations;

public class CreateUpgradeRequestVM
{
    [Required(ErrorMessage = "Address is required!")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters!")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tax code is required!")]
    [StringLength(50, ErrorMessage = "Tax code cannot exceed 50 characters!")]
    public string TaxCode { get; set; } = string.Empty;
}