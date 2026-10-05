using DVLD.Domain.Entities;

namespace DVLD.Application.Patterns.Builder;

/// <summary>
/// Builder Pattern: Step-by-step construction of a TestAppointment entity with validation.
/// </summary>
public class TestAppointmentBuilder
{
    private int _testTypeId;
    private int _localAppId;
    private DateTime _appointmentDate;
    private decimal _paidFees;
    private int _createdByUserId;
    private int? _retakeTestAppId;

    public TestAppointmentBuilder ForTestType(int testTypeId)
    {
        _testTypeId = testTypeId;
        return this;
    }

    public TestAppointmentBuilder ForLocalApplication(int localAppId)
    {
        _localAppId = localAppId;
        return this;
    }

    public TestAppointmentBuilder ScheduledOn(DateTime appointmentDate)
    {
        _appointmentDate = appointmentDate;
        return this;
    }

    public TestAppointmentBuilder WithFees(decimal fees)
    {
        _paidFees = fees;
        return this;
    }

    public TestAppointmentBuilder CreatedBy(int userId)
    {
        _createdByUserId = userId;
        return this;
    }

    public TestAppointmentBuilder WithRetakeApplication(int? retakeAppId)
    {
        _retakeTestAppId = retakeAppId;
        return this;
    }

    public TestAppointment Build()
    {
        if (_testTypeId <= 0)
            throw new InvalidOperationException("Test Appointment requires a valid TestTypeId.");

        if (_localAppId <= 0)
            throw new InvalidOperationException("Test Appointment requires a valid LocalDrivingLicenseApplicationId.");

        return new TestAppointment
        {
            TestTypeId = _testTypeId,
            LocalDrivingLicenseApplicationId = _localAppId,
            AppointmentDate = _appointmentDate,
            PaidFees = _paidFees,
            CreatedByUserId = _createdByUserId,
            IsLocked = false,
            RetakeTestApplicationId = _retakeTestAppId
        };
    }
}
