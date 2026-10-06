using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Application.Patterns.Factory;
using DVLD.Application.Patterns.Observer;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;
using DVLD.Domain.Patterns.Events;

namespace DVLD.Application.Services;

public class LicenseService : ILicenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILicenseFactoryProvider _factoryProvider;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ICurrentUser _currentUser;

    public LicenseService(
        IUnitOfWork unitOfWork,
        ILicenseFactoryProvider factoryProvider,
        IDomainEventDispatcher eventDispatcher,
        ICurrentUser currentUser)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _factoryProvider = factoryProvider;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<LicenseResponseDto> IssueFirstTimeLicenseAsync(IssueFirstTimeLicenseDto dto, CancellationToken cancellationToken = default)
    {
        var localApp = await _unitOfWork.LocalApplications.GetDetailsByIdAsync(dto.LocalDrivingLicenseApplicationId, cancellationToken)
            ?? throw new EntityNotFoundException("LocalDrivingLicenseApplication", dto.LocalDrivingLicenseApplicationId);

        // State Pattern check
        if (!localApp.Application.CanIssueLicense())
        {
            throw new DomainException($"Cannot issue license for application in status '{localApp.Application.ApplicationStatus}'.");
        }

        // Verify all 3 tests passed!
        int passedTests = await _unitOfWork.TestAppointments.GetPassedTestCountAsync(dto.LocalDrivingLicenseApplicationId, cancellationToken);
        if (passedTests < 3)
        {
            throw new DomainException($"Cannot issue license. Applicant has only passed {passedTests} of 3 required tests (Vision, Theory, Practical).");
        }

        // Find or create Driver record for Person
        int personId = localApp.Application.ApplicantPersonId;
        var driver = await _unitOfWork.Drivers.GetByPersonIdAsync(personId, cancellationToken);
        if (driver == null)
        {
            driver = new Driver
            {
                PersonId = personId,
                CreatedDate = DateTime.UtcNow,
                CreatedByUserId = _currentUser.UserId
            };
            await _unitOfWork.Drivers.AddAsync(driver, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // Factory Method creates the license
        var factory = _factoryProvider.GetFactory(EnIssueReason.FirstTime);
        var context = new LicenseCreationContext(
            localApp.ApplicationId,
            driver.DriverId,
            localApp.LicenseClassId,
            localApp.LicenseClass,
            Notes: dto.Notes,
            CreatedByUserId: _currentUser.UserId
        );

        var license = await factory.CreateLicenseAsync(context, cancellationToken);
        await _unitOfWork.Licenses.AddAsync(license, cancellationToken);

        // Transition application state to Completed using State Pattern
        localApp.Application.Complete();
        _unitOfWork.Applications.Update(localApp.Application);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Raise domain event via Observer pattern
        await _eventDispatcher.DispatchAsync(
            new LicenseIssuedEvent(license.LicenseId, driver.DriverId, license.LicenseClassId, EnIssueReason.FirstTime),
            cancellationToken);

        return await MapLicenseToDto(license, cancellationToken);
    }

    public async Task<LicenseResponseDto> RenewLicenseAsync(RenewLicenseDto dto, CancellationToken cancellationToken = default)
    {
        var oldLicense = await _unitOfWork.Licenses.GetDetailsByIdAsync(dto.LicenseId, cancellationToken)
            ?? throw new EntityNotFoundException("License", dto.LicenseId);

        if (!oldLicense.IsActive)
        {
            throw new DomainException("Cannot renew an inactive license.");
        }

        bool isDetained = await _unitOfWork.DetainedLicenses.IsLicenseDetainedAsync(oldLicense.LicenseId, cancellationToken);
        if (isDetained)
        {
            throw new DomainException("Cannot renew a detained license. It must first be released.");
        }

        // Create base Application for renewal
        var baseApplication = new Domain.Entities.Application
        {
            ApplicantPersonId = oldLicense.Driver.PersonId,
            ApplicationDate = DateTime.UtcNow,
            ApplicationTypeId = (int)EnApplicationType.RenewDrivingLicense,
            ApplicationStatus = EnApplicationStatus.Completed,
            LastStatusDate = DateTime.UtcNow,
            PaidFees = 5.00m + oldLicense.LicenseClass.ClassFees,
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.Applications.AddAsync(baseApplication, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Factory Method pattern creates the renewed license and deactivates the old one
        var factory = _factoryProvider.GetFactory(EnIssueReason.Renew);
        var context = new LicenseCreationContext(
            baseApplication.ApplicationId,
            oldLicense.DriverId,
            oldLicense.LicenseClassId,
            oldLicense.LicenseClass,
            PreviousLicense: oldLicense,
            Notes: dto.Notes,
            CreatedByUserId: _currentUser.UserId
        );

        var newLicense = await factory.CreateLicenseAsync(context, cancellationToken);
        _unitOfWork.Licenses.Update(oldLicense);
        await _unitOfWork.Licenses.AddAsync(newLicense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.DispatchAsync(
            new LicenseIssuedEvent(newLicense.LicenseId, newLicense.DriverId, newLicense.LicenseClassId, EnIssueReason.Renew),
            cancellationToken);

        return await MapLicenseToDto(newLicense, cancellationToken);
    }

    public async Task<LicenseResponseDto> ReplaceLostLicenseAsync(ReplaceLostLicenseDto dto, CancellationToken cancellationToken = default)
    {
        var oldLicense = await _unitOfWork.Licenses.GetDetailsByIdAsync(dto.LicenseId, cancellationToken)
            ?? throw new EntityNotFoundException("License", dto.LicenseId);

        if (!oldLicense.IsActive)
        {
            throw new DomainException("Cannot replace an inactive license.");
        }

        var baseApplication = new Domain.Entities.Application
        {
            ApplicantPersonId = oldLicense.Driver.PersonId,
            ApplicationDate = DateTime.UtcNow,
            ApplicationTypeId = (int)EnApplicationType.ReplaceLostDrivingLicense,
            ApplicationStatus = EnApplicationStatus.Completed,
            LastStatusDate = DateTime.UtcNow,
            PaidFees = 5.00m + 10.00m, // Application fee + lost replacement fee
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.Applications.AddAsync(baseApplication, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var factory = _factoryProvider.GetFactory(EnIssueReason.ReplacementForLost);
        var context = new LicenseCreationContext(
            baseApplication.ApplicationId,
            oldLicense.DriverId,
            oldLicense.LicenseClassId,
            oldLicense.LicenseClass,
            PreviousLicense: oldLicense,
            Notes: "Replaced lost license",
            CreatedByUserId: _currentUser.UserId
        );

        var newLicense = await factory.CreateLicenseAsync(context, cancellationToken);
        _unitOfWork.Licenses.Update(oldLicense);
        await _unitOfWork.Licenses.AddAsync(newLicense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.DispatchAsync(
            new LicenseIssuedEvent(newLicense.LicenseId, newLicense.DriverId, newLicense.LicenseClassId, EnIssueReason.ReplacementForLost),
            cancellationToken);

        return await MapLicenseToDto(newLicense, cancellationToken);
    }

    public async Task<LicenseResponseDto> ReplaceDamagedLicenseAsync(ReplaceDamagedLicenseDto dto, CancellationToken cancellationToken = default)
    {
        var oldLicense = await _unitOfWork.Licenses.GetDetailsByIdAsync(dto.LicenseId, cancellationToken)
            ?? throw new EntityNotFoundException("License", dto.LicenseId);

        if (!oldLicense.IsActive)
        {
            throw new DomainException("Cannot replace an inactive license.");
        }

        var baseApplication = new Domain.Entities.Application
        {
            ApplicantPersonId = oldLicense.Driver.PersonId,
            ApplicationDate = DateTime.UtcNow,
            ApplicationTypeId = (int)EnApplicationType.ReplaceDamagedDrivingLicense,
            ApplicationStatus = EnApplicationStatus.Completed,
            LastStatusDate = DateTime.UtcNow,
            PaidFees = 5.00m + 5.00m, // Application fee + damaged replacement fee
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.Applications.AddAsync(baseApplication, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var factory = _factoryProvider.GetFactory(EnIssueReason.ReplacementForDamaged);
        var context = new LicenseCreationContext(
            baseApplication.ApplicationId,
            oldLicense.DriverId,
            oldLicense.LicenseClassId,
            oldLicense.LicenseClass,
            PreviousLicense: oldLicense,
            Notes: "Replaced damaged license",
            CreatedByUserId: _currentUser.UserId
        );

        var newLicense = await factory.CreateLicenseAsync(context, cancellationToken);
        _unitOfWork.Licenses.Update(oldLicense);
        await _unitOfWork.Licenses.AddAsync(newLicense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.DispatchAsync(
            new LicenseIssuedEvent(newLicense.LicenseId, newLicense.DriverId, newLicense.LicenseClassId, EnIssueReason.ReplacementForDamaged),
            cancellationToken);

        return await MapLicenseToDto(newLicense, cancellationToken);
    }

    public async Task<DetainedLicenseResponseDto> DetainLicenseAsync(DetainLicenseDto dto, CancellationToken cancellationToken = default)
    {
        var license = await _unitOfWork.Licenses.GetByIdAsync(dto.LicenseId, cancellationToken)
            ?? throw new EntityNotFoundException("License", dto.LicenseId);

        if (!license.IsActive)
        {
            throw new DomainException("Cannot detain an inactive license.");
        }

        bool alreadyDetained = await _unitOfWork.DetainedLicenses.IsLicenseDetainedAsync(dto.LicenseId, cancellationToken);
        if (alreadyDetained)
        {
            throw new DomainException("License is already detained.");
        }

        var detained = new DetainedLicense
        {
            LicenseId = dto.LicenseId,
            DetainDate = DateTime.UtcNow,
            FineFees = dto.FineFees,
            CreatedByUserId = _currentUser.UserId,
            IsReleased = false
        };

        await _unitOfWork.DetainedLicenses.AddAsync(detained, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DetainedLicenseResponseDto(
            detained.DetainId,
            detained.LicenseId,
            detained.DetainDate,
            detained.FineFees,
            detained.IsReleased,
            detained.ReleaseDate,
            detained.CreatedByUserId,
            detained.ReleasedByUserId,
            detained.ReleaseApplicationId
        );
    }

    public async Task<DetainedLicenseResponseDto> ReleaseDetainedLicenseAsync(ReleaseLicenseDto dto, CancellationToken cancellationToken = default)
    {
        var detained = await _unitOfWork.DetainedLicenses.GetCurrentDetentionByLicenseIdAsync(dto.LicenseId, cancellationToken)
            ?? throw new DomainException($"License #{dto.LicenseId} is not currently detained.");

        var license = await _unitOfWork.Licenses.GetDetailsByIdAsync(dto.LicenseId, cancellationToken)
            ?? throw new EntityNotFoundException("License", dto.LicenseId);

        // Create base Application for release
        var releaseApp = new Domain.Entities.Application
        {
            ApplicantPersonId = license.Driver.PersonId,
            ApplicationDate = DateTime.UtcNow,
            ApplicationTypeId = (int)EnApplicationType.ReleaseDetainedDrivingLicense,
            ApplicationStatus = EnApplicationStatus.Completed,
            LastStatusDate = DateTime.UtcNow,
            PaidFees = 5.00m + detained.FineFees,
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.Applications.AddAsync(releaseApp, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        detained.Release(releaseApp.ApplicationId, _currentUser.UserId, DateTime.UtcNow);
        _unitOfWork.DetainedLicenses.Update(detained);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DetainedLicenseResponseDto(
            detained.DetainId,
            detained.LicenseId,
            detained.DetainDate,
            detained.FineFees,
            detained.IsReleased,
            detained.ReleaseDate,
            detained.CreatedByUserId,
            detained.ReleasedByUserId,
            detained.ReleaseApplicationId
        );
    }

    public async Task<InternationalLicenseResponseDto> IssueInternationalLicenseAsync(IssueInternationalLicenseDto dto, CancellationToken cancellationToken = default)
    {
        var localLicense = await _unitOfWork.Licenses.GetDetailsByIdAsync(dto.LocalLicenseId, cancellationToken)
            ?? throw new EntityNotFoundException("License", dto.LocalLicenseId);

        if (!localLicense.IsActive)
        {
            throw new DomainException("Cannot issue international license from an inactive local license.");
        }

        // Must be Class 3 (Standard Driving License)
        if (localLicense.LicenseClassId != (int)EnLicenseClass.StandardDrivingLicense)
        {
            throw new DomainException("International driving licenses can only be issued using a Class 3 (Standard Driving License - Car License).");
        }

        if (localLicense.IsExpired())
        {
            throw new DomainException("The local license has expired.");
        }

        var existingInternational = await _unitOfWork.InternationalLicenses.GetActiveByDriverIdAsync(localLicense.DriverId, cancellationToken);
        if (existingInternational != null)
        {
            throw new DomainException("Driver already holds an active international license.");
        }

        // Create application
        var application = new Domain.Entities.Application
        {
            ApplicantPersonId = localLicense.Driver.PersonId,
            ApplicationDate = DateTime.UtcNow,
            ApplicationTypeId = (int)EnApplicationType.IssueInternationalLicense,
            ApplicationStatus = EnApplicationStatus.Completed,
            LastStatusDate = DateTime.UtcNow,
            PaidFees = 5.00m + 50.00m,
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.Applications.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var internationalLicense = new InternationalLicense
        {
            ApplicationId = application.ApplicationId,
            DriverId = localLicense.DriverId,
            IssuedUsingLocalLicenseId = localLicense.LicenseId,
            IssueDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddYears(1), // 1 year validity
            IsActive = true,
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.InternationalLicenses.AddAsync(internationalLicense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InternationalLicenseResponseDto(
            internationalLicense.InternationalLicenseId,
            internationalLicense.ApplicationId,
            internationalLicense.DriverId,
            internationalLicense.IssuedUsingLocalLicenseId,
            internationalLicense.IssueDate,
            internationalLicense.ExpirationDate,
            internationalLicense.IsActive,
            internationalLicense.CreatedByUserId
        );
    }

    public async Task<LicenseResponseDto> GetLicenseByIdAsync(int licenseId, CancellationToken cancellationToken = default)
    {
        var license = await _unitOfWork.Licenses.GetDetailsByIdAsync(licenseId, cancellationToken)
            ?? throw new EntityNotFoundException("License", licenseId);

        return await MapLicenseToDto(license, cancellationToken);
    }

    public async Task<IReadOnlyList<LicenseResponseDto>> GetLicensesByDriverIdAsync(int driverId, CancellationToken cancellationToken = default)
    {
        var licenses = await _unitOfWork.Licenses.GetLicensesByDriverIdAsync(driverId, cancellationToken);
        var list = new List<LicenseResponseDto>(licenses.Count);

        foreach (var lic in licenses)
        {
            list.Add(await MapLicenseToDto(lic, cancellationToken));
        }

        return list;
    }

    public async Task<IReadOnlyList<LicenseResponseDto>> GetAllLicensesAsync(CancellationToken cancellationToken = default)
    {
        var licenses = await _unitOfWork.Licenses.GetAllWithDetailsAsync(cancellationToken);
        var detainedIds = await _unitOfWork.DetainedLicenses.GetDetainedLicenseIdsAsync(cancellationToken);

        return licenses.Select(l => ToDto(l, detainedIds.Contains(l.LicenseId))).ToList();
    }

    public async Task<IReadOnlyList<LicenseResponseDto>> GetLicensesByPersonIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        _ = await _unitOfWork.People.GetByIdAsync(personId, cancellationToken)
            ?? throw new EntityNotFoundException("Person", personId);

        // A person becomes a driver when their first license is issued; before that they have none.
        var driver = await _unitOfWork.Drivers.GetByPersonIdAsync(personId, cancellationToken);
        if (driver == null)
        {
            return Array.Empty<LicenseResponseDto>();
        }

        var licenses = await _unitOfWork.Licenses.GetLicensesByDriverIdAsync(driver.DriverId, cancellationToken);
        var detainedIds = await _unitOfWork.DetainedLicenses.GetDetainedLicenseIdsAsync(cancellationToken);
        return licenses.Select(l => ToDto(l, detainedIds.Contains(l.LicenseId))).ToList();
    }

    private async Task<LicenseResponseDto> MapLicenseToDto(License license, CancellationToken cancellationToken)
    {
        bool isDetained = await _unitOfWork.DetainedLicenses.IsLicenseDetainedAsync(license.LicenseId, cancellationToken);
        return ToDto(license, isDetained);
    }

    private static LicenseResponseDto ToDto(License license, bool isDetained)
    {
        return new LicenseResponseDto(
            license.LicenseId,
            license.ApplicationId,
            license.DriverId,
            license.Driver.PersonId,
            license.Driver.Person.FullName,
            license.Driver.Person.NationalNo,
            license.LicenseClassId,
            license.LicenseClass.ClassName,
            license.IssueDate,
            license.ExpirationDate,
            license.Notes,
            license.PaidFees,
            license.IsActive,
            license.IssueReason.ToString(),
            isDetained,
            license.CreatedByUserId
        );
    }
}
