using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelBooking.infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HotelBooking.application.Services.Domains.Auth;

public class JwtAuthService
{
    private readonly string? _key;
    private readonly string? _issuer;
    private readonly string? _audience;
    private readonly HotelBookingDBContext _context;
    public JwtAuthService(IConfiguration Configuration, HotelBookingDBContext db)
    {
        _key = Configuration["jwt:Serect-Key"];
        _issuer = Configuration["jwt:Issuer"];
        _audience = Configuration["jwt:Audience"];
        _context = db;
    }

    public string GenerateToken(User userAfterVerifyPass)
    {
        // Secret key used to sign the token
        var key = Encoding.ASCII.GetBytes(_key);
        // Build the list of claims for the token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userAfterVerifyPass.Id.ToString()), // Default claim for userId
            new Claim(ClaimTypes.Name, userAfterVerifyPass.UserName),               // Default claim for username
            new Claim("Email", userAfterVerifyPass.Email),                           // Custom claim for email
            new Claim("FullName", userAfterVerifyPass.FullName),                     // Custom claim for fullName
            new Claim("Avatar", userAfterVerifyPass.AvatarUrl ?? string.Empty),      // Custom claim for avatar
            // new Claim(ClaimTypes.Role, userLogin.Role),                            // Default claim for Role
            new Claim(JwtRegisteredClaimNames.Sub, userAfterVerifyPass.UserName),   // Token subject
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),       // Unique token ID
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Token issued-at time
        };
        var userRoles = _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userAfterVerifyPass.Id)
            .ToList();
        foreach (var ur in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
        }

        // Create signing credentials using the secret key
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );
        // Configure the token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1), // Token expires after 1 day
            SigningCredentials = credentials,
            Issuer = _issuer,                 // Set Issuer
            Audience = _audience,             // Set Audience
        };
        // Generate the token using JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // Return the encoded token string
        return tokenHandler.WriteToken(token);
    }

    public (string UserName, List<string> Roles) DecodePayloadToken(string token)
    {
        // Check if token is null or empty
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token must not be empty");
        }
        // Create handler and read the token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                       ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value
                       ?? throw new InvalidOperationException("Username not found in token.");

        var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        return (userName, roles);
    }

}
