using DVLD.Domain.Enums;

namespace DVLD.Domain.Entities;

public class TestResultRecord
{
    public int TestId { get; set; }
    public int TestAppointmentId { get; set; }
    public EnTestResult TestResult { get; set; }
    public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation
    public TestAppointment TestAppointment { get; set; } = null!;
}
