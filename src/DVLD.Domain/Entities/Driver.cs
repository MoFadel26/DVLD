namespace DVLD.Domain.Entities;

public class Driver
{
    public int DriverId { get; set; }
    public int PersonId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public int CreatedByUserId { get; set; }

    // Navigations
    public Person Person { get; set; } = null!;
    public ICollection<License> Licenses { get; set; } = new List<License>();
    public ICollection<InternationalLicense> InternationalLicenses { get; set; } = new List<InternationalLicense>();
}
