using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.TemplateMethod;

/// <summary>
/// First test in the pipeline: No prerequisite tests required.
/// </summary>
public class VisionTestWorkflow : BaseTestWorkflow
{
    public override EnTestType TestType => EnTestType.VisionTest;

    public VisionTestWorkflow(IUnitOfWork unitOfWork, ICurrentUser currentUser) : base(unitOfWork, currentUser)
    {
    }

    protected override Task ValidatePrerequisitesAsync(int localAppId, CancellationToken cancellationToken)
    {
        // Vision test is the first test: No prerequisite needed!
        return Task.CompletedTask;
    }
}
