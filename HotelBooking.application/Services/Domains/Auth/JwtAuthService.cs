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

    public string GenerateToken(User userSauKhiVerifyPass)
    {
        // Khóa bí mật để ký token
        var key = Encoding.ASCII.GetBytes(_key);
        // Tạo danh sách các claims cho token
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userSauKhiVerifyPass.Id.ToString()), // Claim mặc định cho userId
            new Claim(ClaimTypes.Name, userSauKhiVerifyPass.UserName),               // Claim mặc định cho username
            new Claim("Email", userSauKhiVerifyPass.Email),               // Claim mặc định cho username
            new Claim("FullName", userSauKhiVerifyPass.FullName),               // Claim tùy chỉnh cho fullName
            new Claim("Avatar", userSauKhiVerifyPass.AvatarUrl ?? string.Empty),               // Claim tùy chỉnh cho avatar
            // new Claim(ClaimTypes.Role, userLogin.Role),                   // Claim mặc định cho Role
            new Claim(JwtRegisteredClaimNames.Sub, userSauKhiVerifyPass.UserName),   // Subject của token
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID của token
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Thời gian tạo token
        };
        var userRoles = _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userSauKhiVerifyPass.Id)
            .ToList();
        foreach (var ur in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
        }
        
        // Tạo khóa bí mật để ký token
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature
        );
        // Thiết lập thông tin cho token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1), // Token hết hạn sau 1 ngày
            SigningCredentials = credentials,
            Issuer = _issuer,                 // Thêm Issuer vào token
            Audience = _audience,              // Thêm Audience vào token
        };
        // Tạo token bằng JwtSecurityTokenHandler
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // Trả về chuỗi token đã mã hóa
        return tokenHandler.WriteToken(token);
    }

    public (string UserName, List<string> Roles) DecodePayloadToken(string token)
    {
        // Kiểm tra token có null hoặc rỗng không
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token không được để trống");
        }
        // Tạo handler và đọc token
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                       ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "UserName")?.Value
                       ?? throw new InvalidOperationException("Username not found in token.");

        var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

        return (userName, roles);
    }

}
