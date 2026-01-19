public class PolicyTypeVM : BaseAdminVM
{

}
public class PolicyVM : BaseAdminVM
{
    public int TypeId { get; set; }

    // Generic Columns (Cho phép Null hết)
    public TimeOnly? TimeFrom { get; set; }
    public TimeOnly? TimeTo { get; set; }

    public int? IntValue1 { get; set; }
    public int? IntValue2 { get; set; }

    public decimal? Amount { get; set; }
    public double? Percent { get; set; }

    public bool? BoolValue { get; set; } = false;
}

public class PolicyCreateOrUpdateVM : BaseCreateOrUpdateAdminVM
{
    public int TypeId { get; set; }

    // Generic Columns (Cho phép Null hết)
    public TimeOnly? TimeFrom { get; set; }
    public TimeOnly? TimeTo { get; set; }

    public int? IntValue1 { get; set; }
    public int? IntValue2 { get; set; }

    public decimal? Amount { get; set; }
    public double? Percent { get; set; }

    public bool? BoolValue { get; set; } = false;
}