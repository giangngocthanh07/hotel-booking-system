using HotelBooking.webapp.Helpers.Common;
using HotelBooking.webapp.Services.Interface;
using HotelBooking.webapp.ViewModels.Form;
using HotelBooking.webapp.ViewModels.Response;
using HotelBooking.webapp.ViewModels.User;

namespace HotelBooking.webapp.Services;

public interface IUserService : ITokenService
{
    Task<ApiResponse<UserDetailVM>> GetCurrentUser();
    Task<ApiResponse<UserDetailVM>> UpdateProfile(int userId, UpdateUserProfileVM vm);
}

public class UserService : IUserService
{
    private readonly HttpClient _http;
    private const string BaseUrl = "v1/auth";

    public UserService(IHttpClientFactory httpClientFactory)
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

    public async Task<ApiResponse<UserDetailVM>> GetCurrentUser()
    {
        return await _http.GetApiAsync<UserDetailVM>($"{BaseUrl}/me");
    }

    public async Task<ApiResponse<UserDetailVM>> UpdateProfile(int userId, UpdateUserProfileVM vm)
    {
        return await _http.PutApiAsync<UserDetailVM, UpdateUserProfileVM>($"{BaseUrl}/profile", vm);
    }
}
