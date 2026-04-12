using System.Text.Json;

public static class BedTypeHelper
{
    // Static readonly shared instance — avoids repeated allocations. Implements STATIC SIMPLE FACTORY PATTERN
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    // --- 1. Map for CREATE (accepts BedTypeCreateDTO) ---
    public static string MapToAdditionalJson(BedTypeCreateDTO dto)
    {
        var data = new BedTypeAdditionalData
        {
            // Business logic: if "Varying Size" selected, reset dimensions to 0
            MinWidth = dto.IsVaryingSize ? 0 : dto.MinWidth,
            MaxWidth = dto.IsVaryingSize ? 0 : dto.MaxWidth
        };
        return JsonSerializer.Serialize(data, _jsonOptions);
    }

    // --- 2. Map for UPDATE (accepts BedTypeUpdateDTO) ---
    public static string MapToAdditionalJson(BedTypeUpdateDTO dto)
    {
        var data = new BedTypeAdditionalData
        {
            // Same logic as Create
            MinWidth = dto.IsVaryingSize ? 0 : dto.MinWidth,
            MaxWidth = dto.IsVaryingSize ? 0 : dto.MaxWidth
        };
        return JsonSerializer.Serialize(data, _jsonOptions);
    }

    // --- 3. Deserialize JSON back to Object (used by GetById/GetAll) ---
    public static BedTypeAdditionalData MapToAdditionalData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return new BedTypeAdditionalData(); // Return empty object (0, 0, false)

        try
        {
            return JsonSerializer.Deserialize<BedTypeAdditionalData>(json, _jsonOptions)
                   ?? new BedTypeAdditionalData();
        }
        catch
        {
            // Guard against malformed JSON — return default to prevent app crash
            return new BedTypeAdditionalData();
        }
    }
}