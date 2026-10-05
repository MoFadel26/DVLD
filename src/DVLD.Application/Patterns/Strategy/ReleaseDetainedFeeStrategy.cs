using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Strategy;

public class ReleaseDetainedFeeStrategy : IFeeCalculationStrategy
{
    private readonly IUnitOfWork _unitOfWork;

    public EnApplicationType ApplicableType => EnApplicationType.ReleaseDetainedDrivingLicense;

    public ReleaseDetainedFeeStrategy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> CalculateTotalFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var appType = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)ApplicableType, cancellationToken)
            ?? throw new EntityNotFoundException("ApplicationType", (int)ApplicableType);

        decimal fineFee = request.AdditionalFee ?? 0m;
        // Application fee ($5) + Fine fees
        return appType.ApplicationFees + fineFee;
    }
}
