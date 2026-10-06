using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DVLD.Infrastructure.Auth;

/// <summary>
/// Token settings shared by token creation and validation. Reads the "Auth" section.
/// Without Auth:JwtKey a random key is generated, so tokens stop working when the API restarts.
/// </summary>
public class JwtSettings
{
    public const string Issuer = "dvld-api";
    public const string Audience = "dvld-web";

    public SymmetricSecurityKey SigningKey { get; }
    public TimeSpan TokenLifetime { get; }
    public bool UsesGeneratedKey { get; }

    public JwtSettings(IConfiguration configuration)
    {
        string? key = configuration["Auth:JwtKey"];
        UsesGeneratedKey = string.IsNullOrWhiteSpace(key);

        byte[] keyBytes = UsesGeneratedKey ? RandomNumberGenerator.GetBytes(64) : Encoding.UTF8.GetBytes(key!);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("Auth:JwtKey must be at least 32 bytes long.");
        }

        SigningKey = new SymmetricSecurityKey(keyBytes);
        TokenLifetime = TimeSpan.FromMinutes(
            int.TryParse(configuration["Auth:TokenLifetimeMinutes"], out int minutes) ? minutes : 480);
    }

    public TokenValidationParameters ValidationParameters => new()
    {
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        IssuerSigningKey = SigningKey,
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
}
