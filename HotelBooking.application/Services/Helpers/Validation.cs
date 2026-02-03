using System.Linq.Expressions;

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    // Thêm thuộc tính này để phân biệt lỗi 400 và 404
    public string StatusCode { get; set; }

    // Singleton cho trường hợp Success để đỡ tốn bộ nhớ new object liên tục
    public static readonly ValidationResult SuccessResult = new ValidationResult { IsValid = true, StatusCode = StatusCodeResponse.Success };

    // Helper tạo nhanh kết quả thành công
    public static ValidationResult Success()
        => SuccessResult;

    // Helper tạo nhanh kết quả thất bại
    public static ValidationResult Fail(string message, string statusCode)
        => new ValidationResult { IsValid = false, Message = message, StatusCode = statusCode };
}


