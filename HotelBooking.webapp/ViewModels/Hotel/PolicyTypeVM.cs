// Model này phải giống với model của Backend
// (Thường được đặt trong một project Shared)
public class PolicyTypeVM
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool? IsDeleted { get; set; }
}