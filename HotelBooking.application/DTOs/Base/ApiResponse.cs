public class ApiResponse<T>
{
    public string? StatusCode { get; set; }
    public string? Message { get; set; }
    public T? Content { get; set; }
}