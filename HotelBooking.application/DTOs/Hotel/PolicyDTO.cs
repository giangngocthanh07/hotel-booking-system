

public class PolicyTypeDTO : BaseAdminDTO
{

}
public class PolicyDTO : BaseAdminDTO
{
    public int TypeId { get; set; }
}


public class PolicyCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
{
    public int TypeId { get; set; }
}
