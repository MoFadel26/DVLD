using DVLD.Domain.Enums;

namespace DVLD.Domain.Entities;

public class License
{
    public int LicenseId { get; set; }
    public int ApplicationId { get; set; }
    public int DriverId { get; set; }
    public int LicenseClassId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string? Notes { get; set; }
    public decimal PaidFees { get; set; }
    public bool IsActive { get; set; }
    public EnIssueReason IssueReason { get; set; }
    public int CreatedByUserId { get; set; }

    // Navigations
    public Application Application { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
    public LicenseClass LicenseClass { get; set; } = null!;
    public ICollection<DetainedLicense> DetainedLicenses { get; set; } = new List<DetainedLicense>();

    public bool IsExpired() => ExpirationDate < DateTime.UtcNow;

    public void Deactivate()
    {
        IsActive = false;
    }
}
