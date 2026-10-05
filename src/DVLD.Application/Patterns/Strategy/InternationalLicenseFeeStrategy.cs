using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Strategy;

public class InternationalLicenseFeeStrategy : IFeeCalculationStrategy
{
    private readonly IUnitOfWork _unitOfWork;

    public EnApplicationType ApplicableType => EnApplicationType.IssueInternationalLicense;

    public InternationalLicenseFeeStrategy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> CalculateTotalFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var appType = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)ApplicableType, cancellationToken)
            ?? throw new EntityNotFoundException("ApplicationType", (int)ApplicableType);

        // Base App fee ($5) + International License Fee ($50)
        decimal internationalFee = 50.00m;
        return appType.ApplicationFees + internationalFee;
    }
}
