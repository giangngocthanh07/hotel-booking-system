using System.ComponentModel.DataAnnotations;

public class CreateServiceAdminVM
{
    [Required(ErrorMessage = "Name is required!")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters!")]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters!")]
    public string? Description { get; set; }
}