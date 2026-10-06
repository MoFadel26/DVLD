using DVLD.Application.Common.Interfaces;
using DVLD.Infrastructure.Data;

namespace DVLD.Infrastructure.Repositories;

/// <summary>
/// Unit of Work Pattern: Coordinates transactional changes across repositories.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly DvldDbContext _context;

    public IUserRepository Users { get; }
    public ICountryRepository Countries { get; }
    public IPersonRepository People { get; }
    public IApplicationRepository Applications { get; }
    public ILocalDrivingLicenseApplicationRepository LocalApplications { get; }
    public ILicenseClassRepository LicenseClasses { get; }
    public ITestAppointmentRepository TestAppointments { get; }
    public ITestResultRepository TestResults { get; }
    public IDriverRepository Drivers { get; }
    public ILicenseRepository Licenses { get; }
    public IInternationalLicenseRepository InternationalLicenses { get; }
    public IDetainedLicenseRepository DetainedLicenses { get; }
    public IApplicationTypeRepository ApplicationTypes { get; }
    public ITestTypeRepository TestTypes { get; }

    public UnitOfWork(
        DvldDbContext context,
        IUserRepository users,
        ICountryRepository countries,
        IPersonRepository people,
        IApplicationRepository applications,
        ILocalDrivingLicenseApplicationRepository localApplications,
        ILicenseClassRepository licenseClasses,
        ITestAppointmentRepository testAppointments,
        ITestResultRepository testResults,
        IDriverRepository drivers,
        ILicenseRepository licenses,
        IInternationalLicenseRepository internationalLicenses,
        IDetainedLicenseRepository detainedLicenses,
        IApplicationTypeRepository applicationTypes,
        ITestTypeRepository testTypes)
    {
        _context = context;
        Users = users;
        Countries = countries;
        People = people;
        Applications = applications;
        LocalApplications = localApplications;
        LicenseClasses = licenseClasses;
        TestAppointments = testAppointments;
        TestResults = testResults;
        Drivers = drivers;
        Licenses = licenses;
        InternationalLicenses = internationalLicenses;
        DetainedLicenses = detainedLicenses;
        ApplicationTypes = applicationTypes;
        TestTypes = testTypes;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
