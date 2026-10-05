using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Strategy;

/// <summary>
/// Strategy Pattern Context: Dynamically selects and executes the appropriate pricing strategy.
/// </summary>
public interface IFeeCalculationContext
{
    Task<decimal> CalculateFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default);
}

public class FeeCalculationContext : IFeeCalculationContext
{
    private readonly IEnumerable<IFeeCalculationStrategy> _strategies;

    public FeeCalculationContext(IEnumerable<IFeeCalculationStrategy> strategies)
    {
        _strategies = strategies;
    }

    public async Task<decimal> CalculateFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var strategy = _strategies.FirstOrDefault(s => s.ApplicableType == request.ApplicationType);
        if (strategy == null)
        {
            throw new InvalidOperationException($"No fee calculation strategy found for application type: {request.ApplicationType}");
        }

        return await strategy.CalculateTotalFeeAsync(request, cancellationToken);
    }
}
