using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Enums;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.Strategy;

public class NewLicenseFeeStrategy : IFeeCalculationStrategy
{
    private readonly IUnitOfWork _unitOfWork;

    public EnApplicationType ApplicableType => EnApplicationType.NewDrivingLicense;

    public NewLicenseFeeStrategy(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<decimal> CalculateTotalFeeAsync(FeeCalculationRequest request, CancellationToken cancellationToken = default)
    {
        var appType = await _unitOfWork.ApplicationTypes.GetByIdAsync((int)ApplicableType, cancellationToken)
            ?? throw new EntityNotFoundException("ApplicationType", (int)ApplicableType);

        if (!request.LicenseClassId.HasValue)
        {
            throw new ArgumentException("LicenseClassId is required for calculating new license fees.");
        }

        var licenseClass = await _unitOfWork.LicenseClasses.GetByIdAsync(request.LicenseClassId.Value, cancellationToken)
            ?? throw new EntityNotFoundException("LicenseClass", request.LicenseClassId.Value);

        // Application Fee ($5) + License Class Fee
        return appType.ApplicationFees + licenseClass.ClassFees;
    }
}
