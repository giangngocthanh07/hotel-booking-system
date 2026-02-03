using System.ComponentModel.DataAnnotations;

public abstract class BaseAdminVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên không được để trống!")]
    [MaxLength(500, ErrorMessage = "Tên quá dài (tối đa 500 ký tự)!")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Mô tả quá dài (tối đa 500 ký tự)!")]
    public string? Description { get; set; }

    public bool? IsDeleted { get; set; }
}

public abstract class BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Tên không được để trống!")]
    [MaxLength(50, ErrorMessage = "Tên quá dài (tối đa 50 ký tự)!")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Mô tả quá dài (tối đa 500 ký tự)!")]
    public string? Description { get; set; }

}