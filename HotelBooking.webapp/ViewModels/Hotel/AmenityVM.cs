using System.ComponentModel.DataAnnotations;

public class AmenityVM
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required!")]
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; } = false;

    // Parse từ Additional JSON

    [Required(ErrorMessage = "Icon class is required!")]
    public string IconClass { get; set; }
    public string IconColor { get; set; } = "blue";
}

