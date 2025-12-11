using System.ComponentModel.DataAnnotations;

public class AmenityVM : BaseAdminVM
{
    // Parse từ Additional JSON

    [Required(ErrorMessage = "Icon class is required!")]
    public string IconClass { get; set; }
    public string IconColor { get; set; } = "blue";
}

public class AmenityCreateOrUpdateVM : BaseCreateOrUpdateAdminVM
{
    [Required(ErrorMessage = "Icon class is required!")]
    public string IconClass { get; set; }
    public string IconColor { get; set; } = "blue";
}
