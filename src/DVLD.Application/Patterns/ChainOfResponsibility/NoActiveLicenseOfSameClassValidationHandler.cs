using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.ChainOfResponsibility;

public class NoActiveLicenseOfSameClassValidationHandler : AbstractValidationHandler<NewApplicationValidationRequest>
{
    private readonly IUnitOfWork _unitOfWork;

    public NoActiveLicenseOfSameClassValidationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task ValidateAsync(NewApplicationValidationRequest request, CancellationToken cancellationToken)
    {
        var existingLicense = await _unitOfWork.Licenses.GetActiveLicenseByPersonAndClassAsync(
            request.PersonId, request.LicenseClassId, cancellationToken);

        if (existingLicense != null)
        {
            throw new ActiveLicenseAlreadyExistsException(request.LicenseClassId);
        }
    }
}
