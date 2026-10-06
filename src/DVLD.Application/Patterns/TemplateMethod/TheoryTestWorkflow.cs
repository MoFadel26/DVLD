using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.TemplateMethod;

/// <summary>
/// Second test in the pipeline: Requires Vision Test to be passed first.
/// </summary>
public class TheoryTestWorkflow : BaseTestWorkflow
{
    public override EnTestType TestType => EnTestType.TheoryTest;

    public TheoryTestWorkflow(IUnitOfWork unitOfWork, ICurrentUser currentUser) : base(unitOfWork, currentUser)
    {
    }

    protected override async Task ValidatePrerequisitesAsync(int localAppId, CancellationToken cancellationToken)
    {
        bool passedVision = await UnitOfWork.TestAppointments.HasPassedTestAsync(localAppId, EnTestType.VisionTest, cancellationToken);
        if (!passedVision)
        {
            throw new PrerequisiteTestNotPassedTestException("Vision Test", "Theory Test");
        }
    }
}
