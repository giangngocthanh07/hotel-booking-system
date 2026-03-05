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


