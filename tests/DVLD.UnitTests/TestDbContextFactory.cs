using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Infrastructure.Data;
using DVLD.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DVLD.UnitTests;

public static class TestDbContextFactory
{
    public static (DvldDbContext Context, IUnitOfWork UnitOfWork) Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<DvldDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new DvldDbContext(options);
        context.Database.EnsureCreated();

        // Seed basic reference data
        if (!context.LicenseClasses.Any())
        {
            context.LicenseClasses.AddRange(
                new LicenseClass { LicenseClassId = 1, ClassName = "Class 1", MinimumAllowedAge = 18, ValidityLength = 5, ClassFees = 15m },
                new LicenseClass { LicenseClassId = 2, ClassName = "Class 2", MinimumAllowedAge = 21, ValidityLength = 5, ClassFees = 30m },
                new LicenseClass { LicenseClassId = 3, ClassName = "Class 3", MinimumAllowedAge = 18, ValidityLength = 10, ClassFees = 20m },
                new LicenseClass { LicenseClassId = 4, ClassName = "Class 4", MinimumAllowedAge = 21, ValidityLength = 10, ClassFees = 200m },
                new LicenseClass { LicenseClassId = 5, ClassName = "Class 5", MinimumAllowedAge = 21, ValidityLength = 10, ClassFees = 50m },
                new LicenseClass { LicenseClassId = 6, ClassName = "Class 6", MinimumAllowedAge = 21, ValidityLength = 10, ClassFees = 250m },
                new LicenseClass { LicenseClassId = 7, ClassName = "Class 7", MinimumAllowedAge = 21, ValidityLength = 10, ClassFees = 300m }
            );
        }

        if (!context.ApplicationTypes.Any())
        {
            context.ApplicationTypes.AddRange(
                new ApplicationType { ApplicationTypeId = 1, ApplicationTypeTitle = "New Driving License", ApplicationFees = 5m },
                new ApplicationType { ApplicationTypeId = 2, ApplicationTypeTitle = "Renew Driving License", ApplicationFees = 5m },
                new ApplicationType { ApplicationTypeId = 3, ApplicationTypeTitle = "Replace Lost", ApplicationFees = 5m },
                new ApplicationType { ApplicationTypeId = 4, ApplicationTypeTitle = "Replace Damaged", ApplicationFees = 5m },
                new ApplicationType { ApplicationTypeId = 5, ApplicationTypeTitle = "Release Detained", ApplicationFees = 5m },
                new ApplicationType { ApplicationTypeId = 6, ApplicationTypeTitle = "Retake Test", ApplicationFees = 5m },
                new ApplicationType { ApplicationTypeId = 7, ApplicationTypeTitle = "Issue International", ApplicationFees = 5m }
            );
        }

        if (!context.TestTypes.Any())
        {
            context.TestTypes.AddRange(
                new TestType { TestTypeId = 1, TestTypeTitle = "Vision Test", TestTypeFees = 10m },
                new TestType { TestTypeId = 2, TestTypeTitle = "Theory Test", TestTypeFees = 20m },
                new TestType { TestTypeId = 3, TestTypeTitle = "Practical Test", TestTypeFees = 30m }
            );
        }

        if (!context.Countries.Any())
        {
            context.Countries.Add(new Country { CountryId = 1, CountryName = "Jordan" });
        }

        context.SaveChanges();

        var peopleRepo = new PersonRepository(context);
        var appRepo = new ApplicationRepository(context);
        var localAppRepo = new LocalDrivingLicenseApplicationRepository(context);
        var licClassRepo = new LicenseClassRepository(context);
        var testAppRepo = new TestAppointmentRepository(context);
        var testResultRepo = new TestResultRepository(context);
        var driverRepo = new DriverRepository(context);
        var licenseRepo = new LicenseRepository(context);
        var intlRepo = new InternationalLicenseRepository(context);
        var detainRepo = new DetainedLicenseRepository(context);
        var appTypeRepo = new ApplicationTypeRepository(context);
        var testTypeRepo = new TestTypeRepository(context);

        var uow = new UnitOfWork(
            context,
            peopleRepo,
            appRepo,
            localAppRepo,
            licClassRepo,
            testAppRepo,
            testResultRepo,
            driverRepo,
            licenseRepo,
            intlRepo,
            detainRepo,
            appTypeRepo,
            testTypeRepo
        );

        return (context, uow);
    }
}
