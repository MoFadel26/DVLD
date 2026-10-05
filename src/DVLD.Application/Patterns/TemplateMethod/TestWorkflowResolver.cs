using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.TemplateMethod;

public interface ITestWorkflowResolver
{
    ITestWorkflow Resolve(EnTestType testType);
}

public class TestWorkflowResolver : ITestWorkflowResolver
{
    private readonly IEnumerable<ITestWorkflow> _workflows;

    public TestWorkflowResolver(IEnumerable<ITestWorkflow> workflows)
    {
        _workflows = workflows;
    }

    public ITestWorkflow Resolve(EnTestType testType)
    {
        var workflow = _workflows.FirstOrDefault(w => w.TestType == testType);
        if (workflow == null)
        {
            throw new NotSupportedException($"No test workflow found for test type: {testType}");
        }
        return workflow;
    }
}
