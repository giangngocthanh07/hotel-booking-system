using System.Net.Http.Json;
using MudBlazor;

public static class HttpClientExtensions
{

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
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER);
        }
    }

    // 2. POST Helper
    public static async Task<ApiResponse<T>> PostApiAsync<T>(this HttpClient client, string url, T data)
    {
        try
        {
            var response = await client.PostAsJsonAsync(url, data);
            return await HandleResponse<T>(response);
        }
        catch
        {
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER);
        }
    }

    // 3. PUT Helper
    public static async Task<ApiResponse<T>> PutApiAsync<T>(this HttpClient client, string url, T data)
    {
        try
        {
            var response = await client.PutAsJsonAsync(url, data);
            return await HandleResponse<T>(response);
        }
        catch
        {
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER);
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
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER);
        }
    }
    // --- Private Helpers để xử lý chung việc đọc JSON ---

    // Dành cho API trả về dữ liệu (GET)
    private static async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            return result ?? ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER);
        }

        // Nếu lỗi (400, 500...), thử đọc message lỗi từ server trả về
        try
        {
            var errorResult = await response.Content.ReadFromJsonAsync<ApiResponse<T>>();
            return errorResult ?? ResponseFactory.Failure<T>(StatusCodeResponse.Error, response.ReasonPhrase ?? MessageResponse.ERROR_IN_SERVER);
        }
        catch
        {
            return ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.ERROR_IN_SERVER);
        }
    }
}