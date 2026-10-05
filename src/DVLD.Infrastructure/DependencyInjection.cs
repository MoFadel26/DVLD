using DVLD.Application.Common.Interfaces;
using DVLD.Infrastructure.Data;
using DVLD.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DVLD.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<DvldDbContext>(options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString) && !connectionString.Contains("InMemory"))
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseInMemoryDatabase("DvldInMemoryDb");
            }
        });

        // Register Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<ILocalDrivingLicenseApplicationRepository, LocalDrivingLicenseApplicationRepository>();
        services.AddScoped<ILicenseClassRepository, LicenseClassRepository>();
        services.AddScoped<ITestAppointmentRepository, TestAppointmentRepository>();
        services.AddScoped<ITestResultRepository, TestResultRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<ILicenseRepository, LicenseRepository>();
        services.AddScoped<IInternationalLicenseRepository, InternationalLicenseRepository>();
        services.AddScoped<IDetainedLicenseRepository, DetainedLicenseRepository>();
        services.AddScoped<IApplicationTypeRepository, ApplicationTypeRepository>();
        services.AddScoped<ITestTypeRepository, TestTypeRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
