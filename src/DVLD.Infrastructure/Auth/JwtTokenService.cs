using System.Security.Claims;
using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DVLD.Infrastructure.Auth;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        DateTime expiresAt = DateTime.UtcNow.Add(_settings.TokenLifetime);

        string token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = JwtSettings.Issuer,
            Audience = JwtSettings.Audience,
            Expires = expiresAt,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            }),
            SigningCredentials = new SigningCredentials(_settings.SigningKey, SecurityAlgorithms.HmacSha256)
        });

        return (token, expiresAt);
    }
}
