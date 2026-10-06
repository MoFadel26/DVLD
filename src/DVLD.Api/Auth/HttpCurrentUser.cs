using DVLD.Application.Common.Interfaces;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DVLD.Api.Auth;

/// <summary>
/// Reads the signed-in user's id from the "sub" claim of the request's JWT.
/// </summary>
public class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int UserId
    {
        get
        {
            string? sub = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(sub, out int userId)
                ? userId
                : throw new UnauthorizedAccessException("The request has no signed-in user.");
        }
    }
}
