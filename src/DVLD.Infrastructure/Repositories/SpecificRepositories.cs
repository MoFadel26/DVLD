using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DVLD.Infrastructure.Repositories;

public class CountryRepository : Repository<Country>, ICountryRepository
{
    public CountryRepository(DvldDbContext context) : base(context)
    {
    }

    public override async Task<IReadOnlyList<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.OrderBy(c => c.CountryName).ToListAsync(cancellationToken);
    }
}

public class PersonRepository : Repository<Person>, IPersonRepository
{
    public PersonRepository(DvldDbContext context) : base(context)
    {
    }

    public override async Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.PersonId == id, cancellationToken);
    }

    public async Task<Person?> GetByNationalNoAsync(string nationalNo, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.NationalNo == nationalNo, cancellationToken);
    }

    public async Task<bool> ExistsByNationalNoAsync(string nationalNo, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(p => p.NationalNo == nationalNo, cancellationToken);
    }
}

public class ApplicationRepository : Repository<Domain.Entities.Application>, IApplicationRepository
{
    public ApplicationRepository(DvldDbContext context) : base(context)
    {
    }

    public override async Task<Domain.Entities.Application?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Person)
            .Include(a => a.ApplicationType)
            .FirstOrDefaultAsync(a => a.ApplicationId == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Domain.Entities.Application>> GetByApplicantPersonIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.ApplicationType)
            .Where(a => a.ApplicantPersonId == personId)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync(cancellationToken);
    }
}

public class LocalDrivingLicenseApplicationRepository : Repository<LocalDrivingLicenseApplication>, ILocalDrivingLicenseApplicationRepository
{
    public LocalDrivingLicenseApplicationRepository(DvldDbContext context) : base(context)
    {
    }

    public async Task<LocalDrivingLicenseApplication?> GetDetailsByIdAsync(int localAppId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(l => l.Application)
                .ThenInclude(a => a.Person)
            .Include(l => l.LicenseClass)
            .Include(l => l.TestAppointments)
                .ThenInclude(ta => ta.TestResultRecord)
            .FirstOrDefaultAsync(l => l.LocalDrivingLicenseApplicationId == localAppId, cancellationToken);
    }

    public async Task<bool> HasActiveApplicationForClassAsync(int personId, int licenseClassId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(l => l.Application)
            .AnyAsync(l => l.Application.ApplicantPersonId == personId
                        && l.LicenseClassId == licenseClassId
                        && l.Application.ApplicationStatus == EnApplicationStatus.New, cancellationToken);
    }

    public async Task<IReadOnlyList<LocalDrivingLicenseApplication>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(l => l.Application)
                .ThenInclude(a => a.Person)
            .Include(l => l.LicenseClass)
            .OrderByDescending(l => l.LocalDrivingLicenseApplicationId)
            .ToListAsync(cancellationToken);
    }
}

public class LicenseClassRepository : Repository<LicenseClass>, ILicenseClassRepository
{
    public LicenseClassRepository(DvldDbContext context) : base(context)
    {
    }
}

public class TestAppointmentRepository : Repository<TestAppointment>, ITestAppointmentRepository
{
    public TestAppointmentRepository(DvldDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TestAppointment>> GetAppointmentsForLocalAppAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ta => ta.TestResultRecord)
            .Where(ta => ta.LocalDrivingLicenseApplicationId == localAppId && ta.TestTypeId == (int)testType)
            .OrderBy(ta => ta.AppointmentDate)
            .ThenBy(ta => ta.TestAppointmentId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TestAppointment?> GetLatestAppointmentAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ta => ta.TestResultRecord)
            .Where(ta => ta.LocalDrivingLicenseApplicationId == localAppId && ta.TestTypeId == (int)testType)
            .OrderByDescending(ta => ta.AppointmentDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasOpenAppointmentAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(ta => ta.LocalDrivingLicenseApplicationId == localAppId
                                       && ta.TestTypeId == (int)testType
                                       && !ta.IsLocked, cancellationToken);
    }

    public async Task<int> GetPassedTestCountAsync(int localAppId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ta => ta.TestResultRecord)
            .Where(ta => ta.LocalDrivingLicenseApplicationId == localAppId
                      && ta.TestResultRecord != null
                      && ta.TestResultRecord.TestResult == EnTestResult.Pass)
            .Select(ta => ta.TestTypeId)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    public async Task<bool> HasPassedTestAsync(int localAppId, EnTestType testType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(ta => ta.TestResultRecord)
            .AnyAsync(ta => ta.LocalDrivingLicenseApplicationId == localAppId
                         && ta.TestTypeId == (int)testType
                         && ta.TestResultRecord != null
                         && ta.TestResultRecord.TestResult == EnTestResult.Pass, cancellationToken);
    }
}

public class TestResultRepository : Repository<TestResultRecord>, ITestResultRepository
{
    public TestResultRepository(DvldDbContext context) : base(context)
    {
    }
}

public class DriverRepository : Repository<Driver>, IDriverRepository
{
    public DriverRepository(DvldDbContext context) : base(context)
    {
    }

    public async Task<Driver?> GetByPersonIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Person)
            .FirstOrDefaultAsync(d => d.PersonId == personId, cancellationToken);
    }
}

public class LicenseRepository : Repository<License>, ILicenseRepository
{
    public LicenseRepository(DvldDbContext context) : base(context)
    {
    }

    public async Task<License?> GetActiveLicenseByPersonAndClassAsync(int personId, int licenseClassId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(l => l.Driver)
            .FirstOrDefaultAsync(l => l.Driver.PersonId == personId
                                   && l.LicenseClassId == licenseClassId
                                   && l.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<License>> GetLicensesByDriverIdAsync(int driverId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(l => l.Driver).ThenInclude(d => d.Person)
            .Include(l => l.LicenseClass)
            .Where(l => l.DriverId == driverId)
            .OrderByDescending(l => l.IssueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<License?> GetDetailsByIdAsync(int licenseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(l => l.Driver).ThenInclude(d => d.Person)
            .Include(l => l.LicenseClass)
            .Include(l => l.Application)
            .FirstOrDefaultAsync(l => l.LicenseId == licenseId, cancellationToken);
    }
}

public class InternationalLicenseRepository : Repository<InternationalLicense>, IInternationalLicenseRepository
{
    public InternationalLicenseRepository(DvldDbContext context) : base(context)
    {
    }

    public async Task<InternationalLicense?> GetActiveByDriverIdAsync(int driverId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(il => il.DriverId == driverId && il.IsActive, cancellationToken);
    }
}

public class DetainedLicenseRepository : Repository<DetainedLicense>, IDetainedLicenseRepository
{
    public DetainedLicenseRepository(DvldDbContext context) : base(context)
    {
    }

    public async Task<DetainedLicense?> GetCurrentDetentionByLicenseIdAsync(int licenseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(dl => dl.LicenseId == licenseId && !dl.IsReleased, cancellationToken);
    }

    public async Task<bool> IsLicenseDetainedAsync(int licenseId, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(dl => dl.LicenseId == licenseId && !dl.IsReleased, cancellationToken);
    }
}

public class ApplicationTypeRepository : Repository<ApplicationType>, IApplicationTypeRepository
{
    public ApplicationTypeRepository(DvldDbContext context) : base(context)
    {
    }
}

public class TestTypeRepository : Repository<TestType>, ITestTypeRepository
{
    public TestTypeRepository(DvldDbContext context) : base(context)
    {
    }
}
