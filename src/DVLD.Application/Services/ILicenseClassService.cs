using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public interface ILicenseClassService
{
    Task<IReadOnlyList<LicenseClassDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<LicenseClassDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
