public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    // Added to distinguish between 400 and 404 errors
    public string? StatusCode { get; set; }

    // Singleton for success case — avoids repeated object allocations
    public static readonly ValidationResult SuccessResult = new ValidationResult { IsValid = true, StatusCode = StatusCodeResponse.Success };

    // Helper to quickly create a success result
    public static ValidationResult Success()
        => SuccessResult;

    // Helper to quickly create a failure result
    public static ValidationResult Fail(string message, string statusCode)
        => new ValidationResult { IsValid = false, Message = message, StatusCode = statusCode };
}

public class ValidationResult<T> : ValidationResult
{
    // Data payload for successful validation
    public T? Data { get; set; }

    // Helper generate success result with data
    public static ValidationResult<T> Success(T data)
        => new ValidationResult<T> { IsValid = true, StatusCode = StatusCodeResponse.Success, Data = data };

    // Override to return ValidationResult<T>
    public static new ValidationResult<T> Fail(string message, string statusCode)
        => new ValidationResult<T> { IsValid = false, Message = message, StatusCode = statusCode };
}


