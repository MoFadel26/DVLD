using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace DVLD.Api.Auth;

/// <summary>
/// Guest sign-in creates a user on every call, so it is limited to 5 per minute per IP address.
/// </summary>
public static class GuestRateLimit
{
    public const string Policy = "guest-sign-in";

    public static IServiceCollection AddGuestRateLimit(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.AddPolicy(Policy, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1) }));

            // Answer with a problem body like every other API error.
            options.OnRejected = (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Detail = "Too many guest sign-ins from this address. Try again in a minute.",
                    Instance = context.HttpContext.Request.Path
                }, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json", cancellationToken));
            };
        });
}
