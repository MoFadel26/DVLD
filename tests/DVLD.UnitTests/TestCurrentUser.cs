using DVLD.Application.Common.Interfaces;

namespace DVLD.UnitTests;

/// <summary>
/// Stands in for the signed-in user in tests.
/// </summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public int UserId { get; init; } = 1;
}
