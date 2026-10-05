namespace DVLD.Domain.Entities;

public class LocalDrivingLicenseApplication
{
    public int LocalDrivingLicenseApplicationId { get; set; }
    public int ApplicationId { get; set; }
    public int LicenseClassId { get; set; }

    // Navigations
    public Application Application { get; set; } = null!;
    public LicenseClass LicenseClass { get; set; } = null!;
    public ICollection<TestAppointment> TestAppointments { get; set; } = new List<TestAppointment>();
}
