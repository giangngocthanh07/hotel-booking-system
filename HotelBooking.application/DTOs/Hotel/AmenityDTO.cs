using System.ComponentModel.DataAnnotations;

public enum AmenityTypeEnum
{
    InRoom = 1,
    BathRoom = 2,
    SafetySecurity = 3,
    General = 4,
    Nearby = 5,
    FoodAndDrink = 6,
}

public class AmenityTypeDTO : BaseAdminDTO
{
    // Parsed from Additional JSON

    // [Required(ErrorMessage = "Icon class is required")]
    public string? IconClass { get; set; }
    public string? IconColor { get; set; }
}

// 1. Display DTO
public class AmenityDTO : BaseAdminDTO
{
    public int TypeId { get; set; }
}

// 2. Add/Edit DTOs
// 1. DTO used for CREATING (Requires TypeId) -> Inherits Base to get Name, Description
public class AmenityCreateDTO : BaseCreateOrUpdateAdminDTO
{
    [Required(ErrorMessage = "Amenity type cannot be empty")]
    public int TypeId { get; set; }
}

// 2. DTO used for UPDATING (No TypeId) -> Clean on Swagger!
public class AmenityUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    // Empty, only gets Name and Description from Base
}
