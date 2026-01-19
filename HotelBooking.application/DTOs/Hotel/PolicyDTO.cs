

public class PolicyTypeDTO : BaseAdminDTO
{

}
public class PolicyDTO : BaseAdminDTO
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


public class PolicyCreateOrUpdateDTO : BaseCreateOrUpdateAdminDTO
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
