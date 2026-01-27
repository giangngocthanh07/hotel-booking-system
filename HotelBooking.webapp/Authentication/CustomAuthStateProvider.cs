

using System.Security.Claims;                   // Xử lý thông tin người dùng
using System.Text.Json;                         // Xử lý dữ liệu JSON
using Microsoft.AspNetCore.Components.Authorization; // Quản lý xác thực
using Blazored.LocalStorage;                    // Lưu trữ token cục bộ
using System.IdentityModel.Tokens.Jwt;          // Giải mã và xử lý JWT
using Microsoft.IdentityModel.Tokens;
using System.Text;                              // Xử lý tác vụ bất đồng bộ
namespace HotelBooking.Client;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
    private IConfiguration _config;

    public CustomAuthStateProvider(ILocalStorageService localStorage, IConfiguration Configuration)
    {
        _localStorage = localStorage;
        _config = Configuration;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Lấy token từ localStorage
            var token = await _localStorage.GetItemAsync<string>("accessToken");
            // Trường hợp không có token => Không đăng nhập
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Cấu hình kiểm tra token
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:Serect-Key"] ?? string.Empty)),
                ValidateIssuer = true,
                ValidIssuer = _config["jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
            // Giải mã token để xác thực
            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return new AuthenticationState(principal);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }


    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync("accessToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}


