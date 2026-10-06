using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
}
