using System.ComponentModel.DataAnnotations;

public class CreateServiceAdminVM
{
    [Required(ErrorMessage = "Vui lòng nhập tên service")]
    [MaxLength(100)]
    public string Name { get; set; }
    public string? Description { get; set; }
}