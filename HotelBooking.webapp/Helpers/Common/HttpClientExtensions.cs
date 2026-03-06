namespace HotelBooking.webapp.Helpers.Common;

using System.Text.Json;

public static class HttpClientExtensions
{
    // 1. [IMPORTANT] Shared JSON Configuration
    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // Ignores case (items == Items)
        PropertyNamingPolicy = null // Preserves original property names if needed
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

    // --- 2. POST HELPERS ---
    
    // a) With Body: Accepts TRequest (input) and returns TResponse (output)
    public static async Task<ApiResponse<TResponse>> PostApiAsync<TResponse, TRequest>(
        this HttpClient client,
        string url,
        TRequest data) // <--- Provides full Type Safety
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

    // b) No Body: Returns TResponse (output) without requiring an input payload
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

    // --- 3. PUT HELPER: Accepts TRequest (input) and returns TResponse (output) ---
    public static async Task<ApiResponse<TResponse>> PutApiAsync<TResponse, TRequest>(
        this HttpClient client,
        string url,
        TRequest data) // <--- Type Safe implementation
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

    // --- Private Helpers for centralized JSON Response handling ---

    private static async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
    {
        // Successful Response (2xx)
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(_options);
            return result ?? ResponseFactory.Failure<T>(StatusCodeResponse.Error, MessageResponse.Common.ERROR_IN_SERVER);
        }

        // Error Handling (400, 500, etc.)
        // Attempt to extract the error message returned by the server
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