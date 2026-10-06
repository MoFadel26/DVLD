using DVLD.Api.Auth;
using DVLD.Api.Middleware;
using DVLD.Application;
using DVLD.Application.Common.Interfaces;
using DVLD.Infrastructure;
using DVLD.Infrastructure.Auth;
using DVLD.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Native ASP.NET Core OpenAPI (no Swagger), with bearer auth declared for Scalar
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// Clean Architecture Layers DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT authentication: every endpoint requires a signed-in user unless marked [AllowAnonymous]
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<JwtSettings>((options, settings) =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = settings.ValidationParameters;
        options.Events = new JwtBearerEvents
        {
            // Reject tokens that were signed out before they expired.
            OnTokenValidated = async context =>
            {
                string? tokenId = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var revokedTokens = context.HttpContext.RequestServices.GetRequiredService<IRevokedTokenRepository>();
                if (tokenId == null || await revokedTokens.IsRevokedAsync(tokenId, context.HttpContext.RequestAborted))
                {
                    context.Fail("The token has been signed out.");
                }
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

var app = builder.Build();

// Custom Domain Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure OpenAPI & Scalar endpoint
if (app.Environment.IsDevelopment())
{
    // Native OpenAPI spec exposed at /openapi/v1.json
    app.MapOpenApi().AllowAnonymous();
    // Scalar interactive API reference UI exposed at /scalar/v1
    app.MapScalarApiReference().AllowAnonymous();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (app.Services.GetRequiredService<JwtSettings>().UsesGeneratedKey)
{
    app.Logger.LogWarning("Auth:JwtKey is not set; using a random signing key. Tokens stop working when the API restarts.");
}

// Seed Database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DvldDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DatabaseSeeder.SeedAsync(context, logger, passwordHasher, builder.Configuration["Auth:SeedAdminPassword"]);
}

app.Run();

// Partial class for WebApplicationFactory in integration tests if needed
public partial class Program { }
