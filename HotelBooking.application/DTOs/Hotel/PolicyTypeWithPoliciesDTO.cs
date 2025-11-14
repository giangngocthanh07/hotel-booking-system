public class PolicyTypeWithPoliciesDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<PolicyDTO> Policies { get; set; } = new();
}