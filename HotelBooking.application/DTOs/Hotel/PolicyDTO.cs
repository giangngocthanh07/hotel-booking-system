
public class PolicyDTO : BaseAdminDTO
{
    public int PolicyTypeId { get; set; }
}

public class PolicyTypeDTO : BaseAdminDTO
{

}

public class PolicyCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public int PolicyTypeId { get; set; }
}

public class MangagePolicyDTO
{
    public List<PolicyTypeDTO> PolicyTypes { get; set; } = new List<PolicyTypeDTO>();

    public int SelectedTypeId { get; set; }
    public string? SelectedTypeName { get; set; }

    public List<PolicyDTO> Policies { get; set; } = new();
}