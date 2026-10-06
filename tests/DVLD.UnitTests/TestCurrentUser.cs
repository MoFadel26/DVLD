using DVLD.Application.Common.Interfaces;

namespace DVLD.UnitTests;

/// <summary>
/// Stands in for the signed-in user in tests.
/// </summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public int UserId { get; init; } = 1;
    public string? TokenId { get; init; } = "test-token";
    public DateTime? TokenExpiresAt { get; init; } = DateTime.UtcNow.AddHours(1);
}
