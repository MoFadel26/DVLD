namespace DVLD.Domain.Entities;

public class InternationalLicense
{
    public int InternationalLicenseId { get; set; }
    public int ApplicationId { get; set; }
    public int DriverId { get; set; }
    public int IssuedUsingLocalLicenseId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public bool IsActive { get; set; }
    public int CreatedByUserId { get; set; }

    // Navigations
    public Application Application { get; set; } = null!;
    public Driver Driver { get; set; } = null!;
    public License IssuedUsingLocalLicense { get; set; } = null!;
}
