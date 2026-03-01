using System.Text.Json;

public static class HttpClientExtensions
{
    // 1. [QUAN TRỌNG] Tạo cấu hình JSON dùng chung
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // Bỏ qua phân biệt hoa thường (items == Items)
        PropertyNamingPolicy = null // Giữ nguyên tên property nếu cần
    };

    // 1. GET Helper
    public static async Task<ApiResponse<T>> GetApiAsync<T>(this HttpClient client, string url)
    {
        try
        {
            var response = await client.GetAsync(url);
            return await HandleResponse<T>(response);
        }
        catch (Exception)
        {
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }
    }

    // --- 2. POST: 
    // ========== a) With Body --- Nhận TResponse (đầu ra) và TRequest (đầu vào) ---
    public static async Task<ApiResponse<TResponse>> PostApiAsync<TResponse, TRequest>(
        this HttpClient client,
        string url,
        TRequest data) // <--- Type Safe tuyệt đối ở đây
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, data, _options);
            return await HandleResponse<TResponse>(response);
        }
        catch
        {
            return ResponseFactory.Failure<TResponse>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }
    }

    // ========== b) No Body --- Nhận TResponse (đầu ra) nhưng không có TRequest (đầu vào)
    public static async Task<ApiResponse<TResponse>> PostApiAsync<TResponse>(
        this HttpClient client,
        string url)
    {
        try
        {
            var response = await client.PostAsync(url, null);
            return await HandleResponse<TResponse>(response);
        }
        catch
        {
            return ResponseFactory.Failure<TResponse>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }
    }

    // --- 3. PUT: Nhận TResponse (đầu ra) và TRequest (đầu vào) ---
    public static async Task<ApiResponse<TResponse>> PutApiAsync<TResponse, TRequest>(
        this HttpClient client,
        string url,
        TRequest data) // <--- Type Safe tuyệt đối ở đây
    {
        try
        {
            var response = await client.PutAsJsonAsync(url, data, _options);
            return await HandleResponse<TResponse>(response);
        }
        catch
        {
            return ResponseFactory.Failure<TResponse>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }
    }

    // 4. DELETE Helper
    public static async Task<ApiResponse<T>> DeleteApiAsync<T>(this HttpClient client, string url)
    {
        try
        {
            var response = await client.DeleteAsync(url);
            return await HandleResponse<T>(response);
        }
        catch
        {
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }
    }
    // --- Private Helpers để xử lý chung việc đọc JSON ---

    // Dành cho API trả về dữ liệu (GET)
    private static async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(_options);
            return result ?? ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }

        // Nếu lỗi (400, 500...), thử đọc message lỗi từ server trả về
        try
        {
            var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(_options);
            return errorResult ?? ResponseFactory.Failure<T>(StatusCodeResponse.Error, response.ReasonPhrase ?? MessageResponse.Common.ERROR_IN_SERVER);
        }
        catch
        {
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }
    }
}