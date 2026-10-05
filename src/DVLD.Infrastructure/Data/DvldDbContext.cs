using DVLD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DVLD.Infrastructure.Data;

public class DvldDbContext : DbContext
{
    public DvldDbContext(DbContextOptions<DvldDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<LicenseClass> LicenseClasses => Set<LicenseClass>();
    public DbSet<ApplicationType> ApplicationTypes => Set<ApplicationType>();
    public DbSet<Domain.Entities.Application> Applications => Set<Domain.Entities.Application>();
    public DbSet<LocalDrivingLicenseApplication> LocalDrivingLicenseApplications => Set<LocalDrivingLicenseApplication>();
    public DbSet<TestType> TestTypes => Set<TestType>();
    public DbSet<TestAppointment> TestAppointments => Set<TestAppointment>();
    public DbSet<TestResultRecord> TestResults => Set<TestResultRecord>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<InternationalLicense> InternationalLicenses => Set<InternationalLicense>();
    public DbSet<DetainedLicense> DetainedLicenses => Set<DetainedLicense>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DvldDbContext).Assembly);
    }
}
