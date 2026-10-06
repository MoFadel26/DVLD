namespace DVLD.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public EntityNotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}

public class AgeRequirementNotMetException : DomainException
{
    public int ApplicantAge { get; }
    public int RequiredAge { get; }

    public AgeRequirementNotMetException(int applicantAge, int requiredAge)
        : base($"Applicant age is {applicantAge} years, but the minimum required age for this license class is {requiredAge} years.")
    {
        ApplicantAge = applicantAge;
        RequiredAge = requiredAge;
    }
}

public class ActiveLicenseAlreadyExistsException : DomainException
{
    public int LicenseClassId { get; }

    public ActiveLicenseAlreadyExistsException(int licenseClassId)
        : base($"The applicant already possesses an active driving license of class {licenseClassId}.")
    {
        LicenseClassId = licenseClassId;
    }
}

public class PendingApplicationAlreadyExistsException : DomainException
{
    public int LicenseClassId { get; }

    public PendingApplicationAlreadyExistsException(int licenseClassId)
        : base($"The applicant already has an active pending application for license class {licenseClassId}.")
    {
        LicenseClassId = licenseClassId;
    }
}

public class PrerequisiteTestNotPassedTestException : DomainException
{
    public string RequiredTest { get; }
    public string AttemptedTest { get; }

    public PrerequisiteTestNotPassedTestException(string requiredTest, string attemptedTest)
        : base($"Cannot schedule or take '{attemptedTest}'. The applicant must first pass the prerequisite '{requiredTest}'.")
    {
        RequiredTest = requiredTest;
        AttemptedTest = attemptedTest;
    }
}

public class InvalidApplicationStateTransitionException : DomainException
{
    public InvalidApplicationStateTransitionException(string message) : base(message)
    {
    }
}

public class ValidationFailedException : DomainException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationFailedException(IReadOnlyList<string> errors)
        : base($"Validation failed: {string.Join("; ", errors)}")
    {
        Errors = errors;
    }

    public ValidationFailedException(string error) : this(new[] { error })
    {
    }
}

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException()
        : base("The username or password is incorrect.")
    {
    }
}
