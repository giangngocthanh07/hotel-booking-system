using System.Text.Json;

public static class BedTypeHelper
{
    // Khai báo static readonly để dùng chung, không cần tạo mới liên tục -> Tối ưu bộ nhớ - ĐÂY LÀ 1 STATIC SIMPLE FACTORY PATTERN
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // --- 1. Map cho CREATE (Nhận BedTypeCreateDTO) ---
    public static string MapToAdditionalJson(BedTypeCreateDTO dto)
    {
        var data = new BedTypeAdditionalData
        {
            // Logic nghiệp vụ: Nếu chọn "Đa dạng" thì reset kích thước về 0
            MinWidth = dto.IsVaryingSize ? 0 : dto.MinWidth,
            MaxWidth = dto.IsVaryingSize ? 0 : dto.MaxWidth
        };
        return JsonSerializer.Serialize(data, _jsonOptions);
    }

    // --- 2. Map cho UPDATE (Nhận BedTypeUpdateDTO) ---
    public static string MapToAdditionalJson(BedTypeUpdateDTO dto)
    {
        var data = new BedTypeAdditionalData
        {
            // Logic tương tự Create
            MinWidth = dto.IsVaryingSize ? 0 : dto.MinWidth,
            MaxWidth = dto.IsVaryingSize ? 0 : dto.MaxWidth
        };
        return JsonSerializer.Serialize(data, _jsonOptions);
    }

    // --- 3. Map ngược từ JSON ra Object (Dùng cho GetById/GetAll) ---
    public static BedTypeAdditionalData MapToAdditionalData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return new BedTypeAdditionalData(); // Trả về object rỗng (0,0, false)

        try
        {
            return JsonSerializer.Deserialize<BedTypeAdditionalData>(json, _jsonOptions)
                   ?? new BedTypeAdditionalData();
        }
        catch
        {
            // Phòng trường hợp JSON lỗi, trả về default để không crash app
            return new BedTypeAdditionalData();
        }
    }
}