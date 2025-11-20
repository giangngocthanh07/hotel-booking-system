using System.ComponentModel.DataAnnotations;

public abstract class UpdateServiceAdminDTO
{
    [Required(ErrorMessage = "Vui lòng nhập tên service")]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class UpdateStandardServiceAdminDTO : UpdateServiceAdminDTO
{
    [Required(ErrorMessage = "Vui lòng nhập đơn vị đo lường")]
    public string Unit { get; set; } = string.Empty;
}

public class UpdateAirportTransferServiceAdminDTO : UpdateServiceAdminDTO
{
}