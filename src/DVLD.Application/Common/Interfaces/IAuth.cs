using DVLD.Domain.Entities;

namespace DVLD.Application.Common.Interfaces;

/// <summary>
/// The signed-in user making the current request.
/// </summary>
public interface ICurrentUser
{
    int UserId { get; }

    /// <summary>The JWT id ("jti") of the token the request was signed with.</summary>
    string? TokenId { get; }

    /// <summary>When the request's token expires.</summary>
    DateTime? TokenExpiresAt { get; }
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
