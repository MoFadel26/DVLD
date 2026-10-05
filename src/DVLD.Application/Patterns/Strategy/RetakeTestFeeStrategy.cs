using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Strategy;

public class RetakeTestFeeStrategy : IFeeCalculationStrategy
{
    private readonly IUnitOfWork _unitOfWork;

    public EnApplicationType ApplicableType => EnApplicationType.RetakeTest;

    public RetakeTestFeeStrategy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> CalculateTotalFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var appType = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)ApplicableType, cancellationToken)
            ?? throw new EntityNotFoundException("ApplicationType", (int)ApplicableType);

        if (!request.TestType.HasValue)
        {
            throw new ArgumentException("TestType is required for calculating retake test fees.");
        }

        var testTypeEntity = await _unitOfWork.TestTypes.GetByIdAsync((int)request.TestType.Value, cancellationToken)
            ?? throw new EntityNotFoundException("TestType", (int)request.TestType.Value);

        // Retake Application Fee ($5) + Test Type Fee ($10 / $20 / $30)
        return appType.ApplicationFees + testTypeEntity.TestTypeFees;
    }
}
