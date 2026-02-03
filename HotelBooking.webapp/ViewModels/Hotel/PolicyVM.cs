using System.ComponentModel.DataAnnotations;

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

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0!")]
    public decimal? Amount { get; set; }
    [Range(0, 100, ErrorMessage = "Tỷ lệ phần trăm phải từ 0 đến 100!")]

    public double? Percent { get; set; }

    public bool? BoolValue { get; set; } = false;
}

// 2. VM Tạo mới (Input)
public class PolicyCreateVM : BaseCreateOrUpdateAdminVM
{
    // [BẮT BUỘC] Khi tạo mới phải biết thuộc loại nào
    [Required(ErrorMessage = "Loại chính sách không được để trống!")]
    public int TypeId { get; set; }

    // --- Các trường dữ liệu (Cho phép null, tùy logic validation sau này) ---

    public TimeOnly? TimeFrom { get; set; }
    public TimeOnly? TimeTo { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Giá trị không được âm!")]
    public int? IntValue1 { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Giá trị không được âm!")]
    public int? IntValue2 { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0!")]
    public decimal? Amount { get; set; }

    [Range(0, 100, ErrorMessage = "Phần trăm phải từ 0 đến 100!")]
    public double? Percent { get; set; }

    public bool? BoolValue { get; set; } = false;
}

// 3. VM Cập nhật (Input)
public class PolicyUpdateVM : BaseCreateOrUpdateAdminVM
{
    // [QUAN TRỌNG] Không có TypeId.
    // Không cho phép đổi loại chính sách khi đang update.

    public TimeOnly? TimeFrom { get; set; }
    public TimeOnly? TimeTo { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Giá trị nguyên không được âm!")]
    public int? IntValue1 { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Giá trị nguyên không được âm!")]
    public int? IntValue2 { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn hoặc bằng 0!")]
    public decimal? Amount { get; set; }

    [Range(0, 100, ErrorMessage = "Phần trăm phải từ 0 đến 100!")]
    public double? Percent { get; set; }

    public bool? BoolValue { get; set; }
}