using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.ChainOfResponsibility;

public class MinimumAgeValidationHandler : AbstractValidationHandler<NewApplicationValidationRequest>
{
    private readonly IUnitOfWork _unitOfWork;

    public MinimumAgeValidationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task ValidateAsync(NewApplicationValidationRequest request, CancellationToken cancellationToken)
    {
        var person = await _unitOfWork.People.GetByIdAsync(request.PersonId, cancellationToken);
        var licenseClass = await _unitOfWork.LicenseClasses.GetByIdAsync(request.LicenseClassId, cancellationToken);

        if (person == null)
        {
            throw new EntityNotFoundException("Person", request.PersonId);
        }

        if (licenseClass == null)
        {
            throw new EntityNotFoundException("LicenseClass", request.LicenseClassId);
        }

        int applicantAge = person.GetAge();
        if (applicantAge < licenseClass.MinimumAllowedAge)
        {
            throw new AgeRequirementNotMetException(applicantAge, licenseClass.MinimumAllowedAge);
        }
    }
}
