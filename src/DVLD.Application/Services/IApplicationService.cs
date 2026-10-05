using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public interface IApplicationService
{
    Task<LocalLicenseApplicationResponseDto> CreateNewLocalLicenseApplicationAsync(CreateNewLocalLicenseApplicationDto dto, CancellationToken cancellationToken = default);
    Task<LocalLicenseApplicationResponseDto> GetLocalApplicationByIdAsync(int localAppId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LocalLicenseApplicationResponseDto>> GetAllLocalApplicationsAsync(CancellationToken cancellationToken = default);
    Task CancelApplicationAsync(int applicationId, CancellationToken cancellationToken = default);
}
