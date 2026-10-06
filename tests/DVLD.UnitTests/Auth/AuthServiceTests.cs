using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Application.Services;
using DVLD.Domain.Entities;
using DVLD.Domain.Exceptions;
using DVLD.Infrastructure.Auth;
using Xunit;

namespace DVLD.UnitTests.Auth;

public class AuthServiceTests
{
    private sealed class FakeTokenService : ITokenService
    {
        public (string Token, DateTime ExpiresAt) CreateToken(User user) => ($"token-for-{user.UserId}", DateTime.UtcNow.AddHours(1));
    }

    private static async Task<AuthService> CreateService(string dbName, bool isActive = true)
    {
        var (context, uow) = TestDbContextFactory.Create(dbName);
        var hasher = new Pbkdf2PasswordHasher();
        context.Users.Add(new User { UserId = 5, Username = "admin", PasswordHash = hasher.Hash("Secret@123"), IsActive = isActive });
        await context.SaveChangesAsync();
        return new AuthService(uow, hasher, new FakeTokenService(), new TestCurrentUser { UserId = 5 });
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsTokenAndUser()
    {
        var service = await CreateService(nameof(Login_WithCorrectPassword_ReturnsTokenAndUser));

        var result = await service.LoginAsync(new LoginDto("ADMIN", "Secret@123"));

        Assert.Equal("token-for-5", result.Token);
        Assert.Equal(new UserDto(5, "admin"), result.User);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsInvalidCredentials()
    {
        var service = await CreateService(nameof(Login_WithWrongPassword_ThrowsInvalidCredentials));

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(new LoginDto("admin", "wrong")));
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(new LoginDto("nobody", "Secret@123")));
    }

    [Fact]
    public async Task Login_DisabledUser_ThrowsInvalidCredentials()
    {
        var service = await CreateService(nameof(Login_DisabledUser_ThrowsInvalidCredentials), isActive: false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => service.LoginAsync(new LoginDto("admin", "Secret@123")));
    }

    [Fact]
    public async Task GetCurrentUser_ReturnsSignedInUser()
    {
        var service = await CreateService(nameof(GetCurrentUser_ReturnsSignedInUser));

        Assert.Equal(new UserDto(5, "admin"), await service.GetCurrentUserAsync());
    }
}
