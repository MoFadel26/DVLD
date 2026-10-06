using DVLD.Domain.Entities;

namespace DVLD.Application.Common.Interfaces;

/// <summary>
/// The signed-in user making the current request.
/// </summary>
public interface ICurrentUser
{
    int UserId { get; }
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
