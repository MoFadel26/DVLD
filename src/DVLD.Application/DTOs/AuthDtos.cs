namespace DVLD.Application.DTOs;

public record LoginDto(
    string Username,
    string Password
);

public record UserDto(
    int UserId,
    string Username
);

public record LoginResponseDto(
    string Token,
    DateTime ExpiresAt,
    UserDto User
);
