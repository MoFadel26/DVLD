using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Strategy;

public class ReplaceDamagedFeeStrategy : IFeeCalculationStrategy
{
    private readonly IUnitOfWork _unitOfWork;

    public EnApplicationType ApplicableType => EnApplicationType.ReplaceDamagedDrivingLicense;

    public ReplaceDamagedFeeStrategy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> CalculateTotalFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var appType = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)ApplicableType, cancellationToken)
            ?? throw new EntityNotFoundException("ApplicationType", (int)ApplicableType);

        // Application fee ($5) + Damaged replacement fee ($5)
        decimal damagedReplacementFee = 5.00m;
        return appType.ApplicationFees + damagedReplacementFee;
    }
}
