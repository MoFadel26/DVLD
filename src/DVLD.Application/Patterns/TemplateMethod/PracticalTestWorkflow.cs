using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.TemplateMethod;

/// <summary>
/// Third test in the pipeline: Requires Theory Test to be passed first.
/// </summary>
public class PracticalTestWorkflow : BaseTestWorkflow
{
    public override EnTestType TestType => EnTestType.PracticalTest;

    public PracticalTestWorkflow(IUnitOfWork unitOfWork, ICurrentUser currentUser) : base(unitOfWork, currentUser)
    {
    }

    protected override async Task ValidatePrerequisitesAsync(int localAppId, CancellationToken cancellationToken)
    {
        bool passedTheory = await UnitOfWork.TestAppointments.HasPassedTestAsync(localAppId, EnTestType.TheoryTest, cancellationToken);
        if (!passedTheory)
        {
            throw new PrerequisiteTestNotPassedTestException("Theory Test", "Practical Test");
        }
    }
}
