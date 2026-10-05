using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.ChainOfResponsibility;

public class NoPendingApplicationOfSameClassValidationHandler : AbstractValidationHandler<NewApplicationValidationRequest>
{
    private readonly IUnitOfWork _unitOfWork;

    public NoPendingApplicationOfSameClassValidationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task ValidateAsync(NewApplicationValidationRequest request, CancellationToken cancellationToken)
    {
        bool hasPending = await _unitOfWork.LocalApplications.HasActiveApplicationForClassAsync(
            request.PersonId, request.LicenseClassId, cancellationToken);

        if (hasPending)
        {
            throw new PendingApplicationAlreadyExistsException(request.LicenseClassId);
        }
    }
}
