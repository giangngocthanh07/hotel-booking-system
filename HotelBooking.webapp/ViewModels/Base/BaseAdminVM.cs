using System.ComponentModel.DataAnnotations;

public abstract class BaseAdminVM
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Name is required!")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsDeleted { get; set; }
}

public abstract class BaseCreateOrUpdateAdminVM
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

}