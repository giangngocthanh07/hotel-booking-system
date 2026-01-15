public class PolicyTypeVM : BaseAdminVM
{

}
public class PolicyVM : BaseAdminVM
{
    public int TypeId { get; set; }
}

public class PolicyCreateOrUpdateVM : BaseCreateOrUpdateAdminVM
{
    public int TypeId { get; set; }
}