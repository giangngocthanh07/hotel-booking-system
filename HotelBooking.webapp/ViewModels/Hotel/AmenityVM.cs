using System.ComponentModel.DataAnnotations;

public class AmenityTypeVM : BaseAdminVM
{
    public string? IconClass { get; set; }
    public string? IconColor { get; set; }
}
public class AmenityVM : BaseAdminVM
{
    public int TypeId { get; set; }
}

public class AmenityCreateOrUpdateVM : BaseCreateOrUpdateAdminVM
{
    public int TypeId { get; set; }
}
