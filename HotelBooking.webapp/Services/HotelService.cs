using HotelBooking.webapp.Helpers.Common;
using HotelBooking.webapp.Services.Interface;
using HotelBooking.webapp.ViewModels.Hotel;
using HotelBooking.webapp.ViewModels.Response;

namespace HotelBooking.webapp.Services;

public interface IHotelService : ITokenService
{
    Task<ApiResponse<IEnumerable<SearchHotelResultDTO>>> SearchHotels(string destination, DateTime? checkin, DateTime? checkout, int adults, int children, int rooms);
    Task<ApiResponse<HotelDetailsVM>> GetHotelDetails(int id);
}

public class HotelService : IHotelService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "v1/Hotel";

    public HotelService(IHttpClientFactory httpClientFactory)
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

    public async Task<ApiResponse<IEnumerable<SearchHotelResultDTO>>> SearchHotels(string destination, DateTime? checkin, DateTime? checkout, int adults, int children, int rooms)
    {
        var url = $"{BaseUrl}/get-search-options?cityName={destination}&checkin={checkin:yyyy-MM-dd}&checkout={checkout:yyyy-MM-dd}&adults={adults}&children={children}&rooms={rooms}";
        return await _http.GetApiAsync<IEnumerable<SearchHotelResultDTO>>(url);
    }

    public async Task<ApiResponse<HotelDetailsVM>> GetHotelDetails(int id)
    {
        return await _http.GetApiAsync<HotelDetailsVM>($"{BaseUrl}/get-hotel-details/{id}");
    }
}
