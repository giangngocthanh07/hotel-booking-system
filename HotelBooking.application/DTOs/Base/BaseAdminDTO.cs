public abstract class BaseAdminDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsDeleted { get; set; } = false;
}

public abstract class BaseCreateOrUpdateAdminDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

}