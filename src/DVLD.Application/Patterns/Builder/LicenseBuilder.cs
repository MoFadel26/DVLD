using DVLD.Domain.Entities;
using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Builder;

public class LicenseBuilder
{
    private int _applicationId;
    private int _driverId;
    private int _licenseClassId;
    private DateTime _issueDate = DateTime.UtcNow;
    private DateTime _expirationDate;
    private string? _notes;
    private decimal _paidFees;
    private bool _isActive = true;
    private EnIssueReason _issueReason = EnIssueReason.FirstTime;
    private int _createdByUserId;

    public LicenseBuilder ForApplication(int applicationId)
    {
        _applicationId = applicationId;
        return this;
    }

    public LicenseBuilder ForDriver(int driverId)
    {
        _driverId = driverId;
        return this;
    }

    public LicenseBuilder ForClass(int licenseClassId)
    {
        _licenseClassId = licenseClassId;
        return this;
    }

    public LicenseBuilder IssuedAt(DateTime issueDate)
    {
        _issueDate = issueDate;
        return this;
    }

    public LicenseBuilder WithValidityPeriod(int validityYears)
    {
        _expirationDate = _issueDate.AddYears(validityYears);
        return this;
    }

    public LicenseBuilder ExpiringAt(DateTime expirationDate)
    {
        _expirationDate = expirationDate;
        return this;
    }

    public LicenseBuilder WithNotes(string? notes)
    {
        _notes = notes;
        return this;
    }

    public LicenseBuilder WithPaidFees(decimal paidFees)
    {
        _paidFees = paidFees;
        return this;
    }

    public LicenseBuilder SetActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public LicenseBuilder WithReason(EnIssueReason reason)
    {
        _issueReason = reason;
        return this;
    }

    public LicenseBuilder CreatedBy(int userId)
    {
        _createdByUserId = userId;
        return this;
    }

    public License Build()
    {
        if (_applicationId <= 0)
            throw new InvalidOperationException("License must be linked to a valid ApplicationId.");

        if (_driverId <= 0)
            throw new InvalidOperationException("License must be linked to a valid DriverId.");

        if (_licenseClassId <= 0)
            throw new InvalidOperationException("License must specify a valid LicenseClassId.");

        if (_expirationDate <= _issueDate)
            throw new InvalidOperationException("Expiration date must be strictly after the issue date.");

        return new License
        {
            ApplicationId = _applicationId,
            DriverId = _driverId,
            LicenseClassId = _licenseClassId,
            IssueDate = _issueDate,
            ExpirationDate = _expirationDate,
            Notes = _notes,
            PaidFees = _paidFees,
            IsActive = _isActive,
            IssueReason = _issueReason,
            CreatedByUserId = _createdByUserId
        };
    }
}
