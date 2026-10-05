using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Services;

public class LicenseClassService : ILicenseClassService
{
    private readonly IUnitOfWork _unitOfWork;

    public LicenseClassService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<LicenseClassDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var classes = await _unitOfWork.LicenseClasses.GetAllAsync(cancellationToken);
        return classes.Select(c => new LicenseClassDto(
            c.LicenseClassId,
            c.ClassName,
            c.ClassDescription,
            c.MinimumAllowedAge,
            c.ValidityLength,
            c.ClassFees
        )).ToList();
    }

    public async Task<LicenseClassDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var lc = await _unitOfWork.LicenseClasses.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("LicenseClass", id);

        return new LicenseClassDto(
            lc.LicenseClassId,
            lc.ClassName,
            lc.ClassDescription,
            lc.MinimumAllowedAge,
            lc.ValidityLength,
            lc.ClassFees
        );
    }
}
