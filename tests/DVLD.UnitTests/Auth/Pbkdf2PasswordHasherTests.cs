using DVLD.Infrastructure.Auth;
using Xunit;

namespace DVLD.UnitTests.Auth;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Verify_AcceptsTheOriginalPasswordOnly()
    {
        string hash = _hasher.Hash("Secret@123");

        Assert.True(_hasher.Verify("Secret@123", hash));
        Assert.False(_hasher.Verify("secret@123", hash));
    }

    [Fact]
    public void Hash_UsesAFreshSaltEachTime()
    {
        Assert.NotEqual(_hasher.Hash("Secret@123"), _hasher.Hash("Secret@123"));
    }

    [Fact]
    public void Verify_RejectsMalformedHash()
    {
        Assert.False(_hasher.Verify("Secret@123", "not-a-hash"));
    }
}
