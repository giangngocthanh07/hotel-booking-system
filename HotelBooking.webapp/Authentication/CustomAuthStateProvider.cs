using System.Security.Claims;                             // User information handling
using Microsoft.AspNetCore.Components.Authorization;      // Authorization management
using Blazored.LocalStorage;                               // Local token storage
using System.IdentityModel.Tokens.Jwt;                    // JWT decoding and processing
using Microsoft.IdentityModel.Tokens;
using System.Text;                                         // Encoding and string processing

namespace HotelBooking.Client;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
    private readonly IConfiguration _config;

    public CustomAuthStateProvider(ILocalStorageService localStorage, IConfiguration configuration)
    {
        _localStorage = localStorage;
        _config = configuration;
    }

    /// <summary>
    /// Core method to determine the current user's authentication state.
    /// Triggered on page refresh or when NotifyAuthenticationStateChanged is called.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Retrieve token from browser's local storage
            var token = await _localStorage.GetItemAsync<string>("accessToken");

            // Case: No token found -> User is not logged in (Anonymous)
            if (string.IsNullOrWhiteSpace(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Define token validation logic
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

            // Validate and decode the JWT to create a ClaimsPrincipal
            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return new AuthenticationState(principal);
        }
        catch (Exception ex)
        {
            // If token is invalid or expired, treat as logged out
            Console.WriteLine($"Token validation failed: {ex.Message}");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Updates local storage with new token and notifies the app of the state change (Logged In).
    /// </summary>
    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync("accessToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Clears the token from local storage and notifies the app of the state change (Logged Out).
    /// </summary>
    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("accessToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}