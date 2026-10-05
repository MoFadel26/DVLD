using DVLD.Domain.Enums;

namespace DVLD.Application.Patterns.Strategy;

/// <summary>
/// Strategy Pattern: Common interface for fee calculation algorithms.
/// </summary>
public interface IFeeCalculationStrategy
{
    EnApplicationType ApplicableType { get; }
    Task<decimal> CalculateTotalFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default);
}
