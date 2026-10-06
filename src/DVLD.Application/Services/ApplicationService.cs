using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Application.Patterns.ChainOfResponsibility;
using DVLD.Application.Patterns.Observer;
using DVLD.Application.Patterns.Strategy;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;
using DVLD.Domain.Patterns.Events;

namespace DVLD.Application.Services;

public class ApplicationService : IApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INewApplicationValidationPipeline _validationPipeline;
    private readonly IFeeCalculationContext _feeContext;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ICurrentUser _currentUser;

    public ApplicationService(
        IUnitOfWork unitOfWork,
        INewApplicationValidationPipeline validationPipeline,
        IFeeCalculationContext feeContext,
        IDomainEventDispatcher eventDispatcher,
        ICurrentUser currentUser)
    {
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _validationPipeline = validationPipeline;
        _feeContext = feeContext;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<LocalLicenseApplicationResponseDto> CreateNewLocalLicenseApplicationAsync(
        CreateNewLocalLicenseApplicationDto dto, CancellationToken cancellationToken = default)
    {
        // 1. Validate through Chain of Responsibility
        var validationRequest = new NewApplicationValidationRequest(dto.ApplicantPersonId, dto.LicenseClassId);
        await _validationPipeline.ValidateAsync(validationRequest, cancellationToken);

        // 2. Calculate fee using Strategy Pattern
        var feeRequest = new FeeCalculationRequest(EnApplicationType.NewDrivingLicense, dto.LicenseClassId);
        decimal totalFee = await _feeContext.CalculateFeeAsync(feeRequest, cancellationToken);

        // 3. Create base Application entity (State is 'New')
        var baseApplication = new Domain.Entities.Application
        {
            ApplicantPersonId = dto.ApplicantPersonId,
            ApplicationDate = DateTime.UtcNow,
            ApplicationTypeId = (int)EnApplicationType.NewDrivingLicense,
            ApplicationStatus = EnApplicationStatus.New,
            LastStatusDate = DateTime.UtcNow,
            PaidFees = 5.00m, // Application base fee is $5
            CreatedByUserId = _currentUser.UserId
        };

        await _unitOfWork.Applications.AddAsync(baseApplication, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 4. Create Local Driving License Application record
        var localApp = new LocalDrivingLicenseApplication
        {
            ApplicationId = baseApplication.ApplicationId,
            LicenseClassId = dto.LicenseClassId
        };

        await _unitOfWork.LocalApplications.AddAsync(localApp, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var person = await _unitOfWork.People.GetByIdAsync(dto.ApplicantPersonId, cancellationToken);
        var licenseClass = await _unitOfWork.LicenseClasses.GetByIdAsync(dto.LicenseClassId, cancellationToken);

        // 5. Trigger domain event
        await _eventDispatcher.DispatchAsync(
            new ApplicationStatusChangedEvent(baseApplication.ApplicationId, EnApplicationStatus.New, EnApplicationStatus.New),
            cancellationToken);

        return new LocalLicenseApplicationResponseDto(
            localApp.LocalDrivingLicenseApplicationId,
            baseApplication.ApplicationId,
            person!.PersonId,
            person.FullName,
            person.NationalNo,
            baseApplication.ApplicationDate,
            licenseClass!.LicenseClassId,
            licenseClass.ClassName,
            0, // New application has 0 passed tests
            baseApplication.ApplicationStatus.ToString(),
            baseApplication.PaidFees
        );
    }

    public async Task<LocalLicenseApplicationResponseDto> GetLocalApplicationByIdAsync(int localAppId, CancellationToken cancellationToken = default)
    {
        var localApp = await _unitOfWork.LocalApplications.GetDetailsByIdAsync(localAppId, cancellationToken)
            ?? throw new EntityNotFoundException("LocalDrivingLicenseApplication", localAppId);

        int passedTestCount = await _unitOfWork.TestAppointments.GetPassedTestCountAsync(localAppId, cancellationToken);

        return new LocalLicenseApplicationResponseDto(
            localApp.LocalDrivingLicenseApplicationId,
            localApp.ApplicationId,
            localApp.Application.Person!.PersonId,
            localApp.Application.Person.FullName,
            localApp.Application.Person.NationalNo,
            localApp.Application.ApplicationDate,
            localApp.LicenseClass.LicenseClassId,
            localApp.LicenseClass.ClassName,
            passedTestCount,
            localApp.Application.ApplicationStatus.ToString(),
            localApp.Application.PaidFees
        );
    }

    public async Task<IReadOnlyList<LocalLicenseApplicationResponseDto>> GetAllLocalApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.LocalApplications.GetAllWithDetailsAsync(cancellationToken);
        var result = new List<LocalLicenseApplicationResponseDto>(list.Count);

        foreach (var localApp in list)
        {
            int passedTests = await _unitOfWork.TestAppointments.GetPassedTestCountAsync(localApp.LocalDrivingLicenseApplicationId, cancellationToken);
            result.Add(new LocalLicenseApplicationResponseDto(
                localApp.LocalDrivingLicenseApplicationId,
                localApp.ApplicationId,
                localApp.Application.Person!.PersonId,
                localApp.Application.Person.FullName,
                localApp.Application.Person.NationalNo,
                localApp.Application.ApplicationDate,
                localApp.LicenseClass.LicenseClassId,
                localApp.LicenseClass.ClassName,
                passedTests,
                localApp.Application.ApplicationStatus.ToString(),
                localApp.Application.PaidFees
            ));
        }

        return result;
    }

    public async Task CancelApplicationAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var app = await _unitOfWork.Applications.GetByIdAsync(applicationId, cancellationToken)
            ?? throw new EntityNotFoundException("Application", applicationId);

        var oldStatus = app.ApplicationStatus;

        // State Pattern handles validation and state change!
        app.Cancel();

        _unitOfWork.Applications.Update(app);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _eventDispatcher.DispatchAsync(
            new ApplicationStatusChangedEvent(app.ApplicationId, oldStatus, app.ApplicationStatus),
            cancellationToken);
    }
}
