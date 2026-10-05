namespace DVLD.Domain.Entities;

public class TestAppointment
{
    public int TestAppointmentId { get; set; }
    public int TestTypeId { get; set; }
    public int LocalDrivingLicenseApplicationId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public decimal PaidFees { get; set; }
    public int CreatedByUserId { get; set; }
    public bool IsLocked { get; set; }
    public int? RetakeTestApplicationId { get; set; }

    // Navigations
    public TestType TestType { get; set; } = null!;
    public LocalDrivingLicenseApplication LocalDrivingLicenseApplication { get; set; } = null!;
    public TestResultRecord? TestResultRecord { get; set; }

    public void Lock()
    {
        IsLocked = true;
    }
}
