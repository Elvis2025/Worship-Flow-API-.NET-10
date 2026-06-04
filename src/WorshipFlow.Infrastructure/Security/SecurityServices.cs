using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WorshipFlow.Application.Abstractions;
using WorshipFlow.Domain.Entities;

namespace WorshipFlow.Infrastructure.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

public sealed class JwtTokenService(IConfiguration configuration) : ITokenService
{
    public TokenPair CreateTokenPair(AppUser user, string? deviceId, string? deviceName, string? ipAddress, string? userAgent)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "dev-key-change-me-dev-key-change-me-32"));
        var expires = DateTimeOffset.UtcNow.AddMinutes(int.TryParse(configuration["Jwt:AccessMinutes"], out var minutes) ? minutes : 30);
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim("tenant_id", user.TenantId.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim(JwtRegisteredClaimNames.Name, user.FullName), new Claim("device_id", deviceId ?? string.Empty) };
        var jwt = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"], claims, expires: expires.UtcDateTime, signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        return new TokenPair(new JwtSecurityTokenHandler().WriteToken(jwt), refreshToken, expires, DateTimeOffset.UtcNow.AddDays(30));
    }

    public string HashToken(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
    }
}
