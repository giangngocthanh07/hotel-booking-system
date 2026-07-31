using HotelBooking.webapp.Helpers.Common;
using HotelBooking.webapp.Services.Interface;
using HotelBooking.webapp.ViewModels.Admin;
using HotelBooking.webapp.ViewModels.Hotel;
using HotelBooking.webapp.ViewModels.Response;

namespace HotelBooking.webapp.Services;

public interface IDashboardService : ITokenService
{
    Task<ApiResponse<AdminDashboardVM>> GetAdminDashboardStats();
    Task<ApiResponse<OwnerDashboardVM>> GetOwnerDashboardStats();
}

public class DashboardService : IDashboardService
{
    private readonly HttpClient _http;

    public DashboardService(IHttpClientFactory httpClientFactory)
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

    public async Task<ApiResponse<AdminDashboardVM>> GetAdminDashboardStats()
    {
        return await _http.GetApiAsync<AdminDashboardVM>("v1/dashboard/stats");
    }

    public async Task<ApiResponse<OwnerDashboardVM>> GetOwnerDashboardStats()
    {
        return await _http.GetApiAsync<OwnerDashboardVM>("v1/owner-dashboard/stats");
    }
}
