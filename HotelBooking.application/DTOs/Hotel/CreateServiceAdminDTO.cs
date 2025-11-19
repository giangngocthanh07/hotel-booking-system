using System.ComponentModel.DataAnnotations;

public abstract class CreateServiceAdminDTO
{
    [Required(ErrorMessage = "Vui lòng nhập tên service")]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

}

public class CreateStandardServiceAdminDTO : CreateServiceAdminDTO
{
    [Required(ErrorMessage = "Vui lòng nhập đơn vị đo lường")]
    public string Unit { get; set; } = string.Empty;
}

public class CreateAirportTransferServiceAdminDTO : CreateServiceAdminDTO
{
    
}