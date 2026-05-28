using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using domain.entities;
using Microsoft.IdentityModel.Tokens;
using services.interfaces;

namespace infrastructure.auth;

public sealed class JwtTokenService(JwtSettings jwtSettings) : ITokenService
{
    public string GenerateJwtToken(User user)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtSettings.Key));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: jwtSettings.Issuer,
            audience: jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
