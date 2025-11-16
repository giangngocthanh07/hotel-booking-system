public class PolicyCreateOrUpdateDTO
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public bool? IsDeleted { get; set; } = false;
    public int PolicyTypeId { get; set; }
}