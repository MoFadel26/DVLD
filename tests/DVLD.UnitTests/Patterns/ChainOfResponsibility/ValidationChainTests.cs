using DVLD.Application.Patterns.ChainOfResponsibility;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;
using Xunit;

namespace DVLD.UnitTests.Patterns.ChainOfResponsibility;

public class ValidationChainTests
{
    [Fact]
    public async Task ValidateAsync_WhenPersonDoesNotExist_ThrowsEntityNotFoundException()
    {
        // Arrange
        var (_, uow) = TestDbContextFactory.Create(nameof(ValidateAsync_WhenPersonDoesNotExist_ThrowsEntityNotFoundException));
        var pipeline = new NewApplicationValidationPipeline(
            new PersonExistsValidationHandler(uow),
            new MinimumAgeValidationHandler(uow),
            new NoActiveLicenseOfSameClassValidationHandler(uow),
            new NoPendingApplicationOfSameClassValidationHandler(uow)
        );

        var request = new NewApplicationValidationRequest(PersonId: 999, LicenseClassId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => pipeline.ValidateAsync(request));
    }

    [Fact]
    public async Task ValidateAsync_WhenApplicantIsUnderage_ThrowsAgeRequirementNotMetException()
    {
        // Arrange
        var (context, uow) = TestDbContextFactory.Create(nameof(ValidateAsync_WhenApplicantIsUnderage_ThrowsAgeRequirementNotMetException));
        var underagePerson = new Person
        {
            PersonId = 1,
            NationalNo = "UNDERAGE_01",
            FirstName = "Kid",
            SecondName = "A",
            LastName = "B",
            DateOfBirth = DateTime.UtcNow.AddYears(-16), // 16 years old
            Gender = EnGender.Male,
            NationalityCountryId = 1
        };
        context.People.Add(underagePerson);
        await context.SaveChangesAsync();

        var pipeline = new NewApplicationValidationPipeline(
            new PersonExistsValidationHandler(uow),
            new MinimumAgeValidationHandler(uow),
            new NoActiveLicenseOfSameClassValidationHandler(uow),
            new NoPendingApplicationOfSameClassValidationHandler(uow)
        );

        // Class 3 requires 18 years
        var request = new NewApplicationValidationRequest(PersonId: 1, LicenseClassId: 3);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AgeRequirementNotMetException>(() => pipeline.ValidateAsync(request));
        Assert.Equal(16, ex.ApplicantAge);
        Assert.Equal(18, ex.RequiredAge);
    }

    [Fact]
    public async Task ValidateAsync_WhenApplicantAlreadyHasActiveLicenseOfSameClass_ThrowsActiveLicenseAlreadyExistsException()
    {
        // Arrange
        var (context, uow) = TestDbContextFactory.Create(nameof(ValidateAsync_WhenApplicantAlreadyHasActiveLicenseOfSameClass_ThrowsActiveLicenseAlreadyExistsException));
        var person = new Person
        {
            PersonId = 2,
            NationalNo = "LIC_HOLDER_01",
            FirstName = "Adult",
            SecondName = "A",
            LastName = "B",
            DateOfBirth = DateTime.UtcNow.AddYears(-25),
            Gender = EnGender.Male,
            NationalityCountryId = 1
        };
        context.People.Add(person);

        var driver = new Driver { DriverId = 1, PersonId = 2 };
        context.Drivers.Add(driver);

        var app = new Domain.Entities.Application
        {
            ApplicationId = 1,
            ApplicantPersonId = 2,
            ApplicationTypeId = 1,
            ApplicationStatus = EnApplicationStatus.Completed
        };
        context.Applications.Add(app);

        var license = new License
        {
            LicenseId = 1,
            ApplicationId = 1,
            DriverId = 1,
            LicenseClassId = 3,
            IssueDate = DateTime.UtcNow.AddYears(-1),
            ExpirationDate = DateTime.UtcNow.AddYears(9),
            IsActive = true,
            IssueReason = EnIssueReason.FirstTime
        };
        context.Licenses.Add(license);
        await context.SaveChangesAsync();

        var pipeline = new NewApplicationValidationPipeline(
            new PersonExistsValidationHandler(uow),
            new MinimumAgeValidationHandler(uow),
            new NoActiveLicenseOfSameClassValidationHandler(uow),
            new NoPendingApplicationOfSameClassValidationHandler(uow)
        );

        var request = new NewApplicationValidationRequest(PersonId: 2, LicenseClassId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<ActiveLicenseAlreadyExistsException>(() => pipeline.ValidateAsync(request));
    }

    [Fact]
    public async Task ValidateAsync_WhenApplicantHasPendingApplicationOfSameClass_ThrowsPendingApplicationAlreadyExistsException()
    {
        // Arrange
        var (context, uow) = TestDbContextFactory.Create(nameof(ValidateAsync_WhenApplicantHasPendingApplicationOfSameClass_ThrowsPendingApplicationAlreadyExistsException));
        var person = new Person
        {
            PersonId = 3,
            NationalNo = "PENDING_APP_01",
            FirstName = "Pending",
            SecondName = "A",
            LastName = "B",
            DateOfBirth = DateTime.UtcNow.AddYears(-22),
            Gender = EnGender.Male,
            NationalityCountryId = 1
        };
        context.People.Add(person);

        var pendingBaseApp = new Domain.Entities.Application
        {
            ApplicationId = 10,
            ApplicantPersonId = 3,
            ApplicationTypeId = 1,
            ApplicationStatus = EnApplicationStatus.New // Still pending!
        };
        context.Applications.Add(pendingBaseApp);

        var pendingLocalApp = new LocalDrivingLicenseApplication
        {
            LocalDrivingLicenseApplicationId = 1,
            ApplicationId = 10,
            LicenseClassId = 3
        };
        context.LocalDrivingLicenseApplications.Add(pendingLocalApp);
        await context.SaveChangesAsync();

        var pipeline = new NewApplicationValidationPipeline(
            new PersonExistsValidationHandler(uow),
            new MinimumAgeValidationHandler(uow),
            new NoActiveLicenseOfSameClassValidationHandler(uow),
            new NoPendingApplicationOfSameClassValidationHandler(uow)
        );

        var request = new NewApplicationValidationRequest(PersonId: 3, LicenseClassId: 3);

        // Act & Assert
        await Assert.ThrowsAsync<PendingApplicationAlreadyExistsException>(() => pipeline.ValidateAsync(request));
    }

    [Fact]
    public async Task ValidateAsync_WhenAllConditionsMet_PassesWithoutException()
    {
        // Arrange
        var (context, uow) = TestDbContextFactory.Create(nameof(ValidateAsync_WhenAllConditionsMet_PassesWithoutException));
        var person = new Person
        {
            PersonId = 4,
            NationalNo = "VALID_APPLICANT",
            FirstName = "Valid",
            SecondName = "A",
            LastName = "B",
            DateOfBirth = DateTime.UtcNow.AddYears(-20),
            Gender = EnGender.Male,
            NationalityCountryId = 1
        };
        context.People.Add(person);
        await context.SaveChangesAsync();

        var pipeline = new NewApplicationValidationPipeline(
            new PersonExistsValidationHandler(uow),
            new MinimumAgeValidationHandler(uow),
            new NoActiveLicenseOfSameClassValidationHandler(uow),
            new NoPendingApplicationOfSameClassValidationHandler(uow)
        );

        var request = new NewApplicationValidationRequest(PersonId: 4, LicenseClassId: 3);

        // Act
        var exception = await Record.ExceptionAsync(() => pipeline.ValidateAsync(request));

        // Assert
        Assert.Null(exception);
    }
}
