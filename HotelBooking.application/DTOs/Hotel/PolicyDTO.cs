public class PolicyDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool? IsDeleted { get; set; }
    public int PolicyTypeId { get; set; }
}

public class PolicyTypeDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool? IsDeleted { get; set; }
}