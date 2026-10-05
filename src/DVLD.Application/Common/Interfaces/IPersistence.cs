using System.Linq.Expressions;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;

namespace DVLD.Application.Common.Interfaces;

/// <summary>
/// Repository Pattern: Generic interface for data persistence operations.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}

public interface IPersonRepository : IRepository<Person>
{
    Task<Person?> GetByNationalNoAsync(string nationalNo, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNationalNoAsync(string nationalNo, CancellationToken cancellationToken = default);
}

public interface IApplicationRepository : IRepository<Domain.Entities.Application>
{
    Task<IReadOnlyList<Domain.Entities.Application>> GetByApplicantPersonIdAsync(int personId, CancellationToken cancellationToken = default);
}

public interface ILocalDrivingLicenseApplicationRepository : IRepository<LocalDrivingLicenseApplication>
{
    Task<LocalDrivingLicenseApplication?> GetDetailsByIdAsync(int localAppId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveApplicationForClassAsync(int personId, int licenseClassId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocalDrivingLicenseApplication>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
}

public interface ILicenseClassRepository : IRepository<LicenseClass>
{
}

public interface ITestAppointmentRepository : IRepository<TestAppointment>
{
    Task<IReadOnlyList<TestAppointment>> GetAppointmentsForLocalAppAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default);
    Task<TestAppointment?> GetLatestAppointmentAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default);
    Task<bool> HasOpenAppointmentAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default);
    Task<int> GetPassedTestCountAsync(int localAppId, CancellationToken cancellationToken = default);
    Task<bool> HasPassedTestAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default);
}

public interface ITestResultRepository : IRepository<TestResultRecord>
{
}

public interface IDriverRepository : IRepository<Driver>
{
    Task<Driver?> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default);
}

public interface ILicenseRepository : IRepository<License>
{
    Task<License?> GetActiveLicenseByPersonAndClassAsync(int personId, int licenseClassId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<License>> GetLicensesByDriverIdAsync(int driverId, CancellationToken cancellationToken = default);
    Task<License?> GetDetailsByIdAsync(int licenseId, CancellationToken cancellationToken = default);
}

public interface IInternationalLicenseRepository : IRepository<InternationalLicense>
{
    Task<InternationalLicense?> GetActiveByDriverIdAsync(int driverId, CancellationToken cancellationToken = default);
}

public interface IDetainedLicenseRepository : IRepository<DetainedLicense>
{
    Task<DetainedLicense?> GetCurrentDetentionByLicenseIdAsync(int licenseId, CancellationToken cancellationToken = default);
    Task<bool> IsLicenseDetainedAsync(int licenseId, CancellationToken cancellationToken = default);
}

public interface IApplicationTypeRepository : IRepository<ApplicationType>
{
}

public interface ITestTypeRepository : IRepository<TestType>
{
}

/// <summary>
/// Unit of Work Pattern: Coordinates transactions across multiple repositories.
/// </summary>
public interface IUnitOfWork
{
    IPersonRepository People { get; }
    IApplicationRepository Applications { get; }
    ILocalDrivingLicenseApplicationRepository LocalApplications { get; }
    ILicenseClassRepository LicenseClasses { get; }
    ITestAppointmentRepository TestAppointments { get; }
    ITestResultRepository TestResults { get; }
    IDriverRepository Drivers { get; }
    ILicenseRepository Licenses { get; }
    IInternationalLicenseRepository InternationalLicenses { get; }
    IDetainedLicenseRepository DetainedLicenses { get; }
    IApplicationTypeRepository ApplicationTypes { get; }
    ITestTypeRepository TestTypes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
