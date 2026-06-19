using HotelBooking.webapp.Helpers.Common;
using HotelBooking.webapp.Services.Interface;
using HotelBooking.webapp.ViewModels.Booking;
using HotelBooking.webapp.ViewModels.Response;

namespace HotelBooking.webapp.Services;

public interface IBookingService : ITokenService
{
    Task<ApiResponse<IEnumerable<BookingHistoryVM>>> GetMyBookingHistory(string? status = null);
    Task<ApiResponse<IEnumerable<BookingHistoryVM>>> GetOwnerBookingManagement(string? status = null, string? searchTerm = null);
}

public class BookingService : IBookingService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "v1/Booking";

    public BookingService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("HotelBookingAPI");
    }

    public void SetToken(string token)
    {
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<ApiResponse<IEnumerable<BookingHistoryVM>>> GetMyBookingHistory(string? status = null)
    {
        var url = $"{BaseUrl}/my-history";
        if (!string.IsNullOrEmpty(status)) url += $"?status={status}";
        return await _http.GetApiAsync<IEnumerable<BookingHistoryVM>>(url);
    }

    public async Task<ApiResponse<IEnumerable<BookingHistoryVM>>> GetOwnerBookingManagement(string? status = null, string? searchTerm = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
        if (!string.IsNullOrEmpty(searchTerm)) queryParams.Add($"searchTerm={searchTerm}");

        var url = $"{BaseUrl}/owner-management";
        if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

        return await _http.GetApiAsync<IEnumerable<BookingHistoryVM>>(url);
    }
}
