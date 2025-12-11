public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    // Thêm thuộc tính này để phân biệt lỗi 400 và 404
    public string StatusCode { get; set; } = StatusCodeResponse.BadRequest;

    // Helper tạo nhanh kết quả thành công
    public static ValidationResult Success()
        => new ValidationResult { IsValid = true };

    // Helper tạo nhanh kết quả thất bại
    public static ValidationResult Fail(string message, string statusCode)
        => new ValidationResult { IsValid = false, Message = message, StatusCode = statusCode };
}

// Static Factory Method
public static class ValidateUtils
{
    /// <summary>
    /// Hàm gốc: Yêu cầu điều kiện 'condition' phải là TRUE. Nếu FALSE thì trả về lỗi.
    /// Trả về null nếu thành công (để dùng toán tử ??)
    /// </summary>
    public static ValidationResult? Require(bool condition, string errorMessage, string statusCode)
    {
        // Nếu điều kiện đúng -> Trả về null (Pass)
        if (condition) return null;

        // Nếu sai -> Trả về lỗi
        return ValidationResult.Fail(errorMessage, statusCode);
    }

    /// <summary>
    /// Helper: Yêu cầu chuỗi không được rỗng
    /// </summary>
    public static ValidationResult? RequireNotEmpty(string value, string errorMessage, string statusCode)
    {
        return Require(!string.IsNullOrWhiteSpace(value), errorMessage, statusCode);
    }

    /// <summary>
    /// Helper: Yêu cầu object không được null
    /// </summary>
    public static ValidationResult? RequireFound(object? obj, string errorMessage, string statusCode)
    {
        // Logic: Nếu obj khác null -> Pass (null)
        // Nếu obj null -> Fail với statusCode (mặc định là 404)
        return Require(obj != null, errorMessage, statusCode);
    }
}