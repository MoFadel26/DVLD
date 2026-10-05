using DVLD.Api.Middleware;
using DVLD.Application;
using DVLD.Infrastructure;
using DVLD.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Native ASP.NET Core OpenAPI (no Swagger)
builder.Services.AddOpenApi();

// Clean Architecture Layers DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Custom Domain Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure OpenAPI & Scalar endpoint
if (app.Environment.IsDevelopment())
{
    // Native OpenAPI spec exposed at /openapi/v1.json
    app.MapOpenApi();
    // Scalar interactive API reference UI exposed at /scalar/v1
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seed Database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DvldDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await DatabaseSeeder.SeedAsync(context, logger);
}

app.Run();

// Partial class for WebApplicationFactory in integration tests if needed
public partial class Program { }
