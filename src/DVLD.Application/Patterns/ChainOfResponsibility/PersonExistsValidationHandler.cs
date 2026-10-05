using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Patterns.ChainOfResponsibility;

public class PersonExistsValidationHandler : AbstractValidationHandler<NewApplicationValidationRequest>
{
    private readonly IUnitOfWork _unitOfWork;

    public PersonExistsValidationHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task ValidateAsync(NewApplicationValidationRequest request, CancellationToken cancellationToken)
    {
        var person = await _unitOfWork.People.GetByIdAsync(request.PersonId, cancellationToken);
        if (person == null)
        {
            throw new EntityNotFoundException("Person", request.PersonId);
        }
    }
}
