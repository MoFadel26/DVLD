using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUser _currentUser;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _currentUser = currentUser;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(dto.Username.Trim(), cancellationToken);

        // Same error for an unknown user, a wrong password, and a disabled account,
        // so the response does not reveal which usernames exist.
        if (user == null || !user.IsActive || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return new LoginResponseDto(token, expiresAt, new UserDto(user.UserId, user.Username));
    }

    public async Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUser.UserId, cancellationToken)
            ?? throw new EntityNotFoundException("User", _currentUser.UserId);
        return new UserDto(user.UserId, user.Username);
    }
}
