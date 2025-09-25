using System.ComponentModel.DataAnnotations;

public class AmenityCreateOrUpdateDTO
{
    [Required(ErrorMessage = "The Name field is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "The IconClass field is required.")]
    public string IconClass { get; set; } = string.Empty;
    public string IconColor { get; set; } = "blue";
}
