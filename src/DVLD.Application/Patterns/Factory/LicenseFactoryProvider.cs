using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Factory;

/// <summary>
/// Factory Provider: Registry that yields the correct license factory for a given issue reason.
/// </summary>
public interface ILicenseFactoryProvider
{
    ILicenseFactory GetFactory(EnIssueReason reason);
}

public class LicenseFactoryProvider : ILicenseFactoryProvider
{
    private readonly IEnumerable<ILicenseFactory> _factories;

    public LicenseFactoryProvider(IEnumerable<ILicenseFactory> factories)
    {
        _factories = factories;
    }

    public ILicenseFactory GetFactory(EnIssueReason reason)
    {
        var factory = _factories.FirstOrDefault(f => f.SupportedReason == reason);
        if (factory == null)
        {
            throw new NotSupportedException($"No license factory registered for reason: {reason}");
        }
        return factory;
    }
}
