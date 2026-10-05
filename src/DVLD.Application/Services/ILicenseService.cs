using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public interface ILicenseService
{
    Task<LicenseResponseDto> IssueFirstTimeLicenseAsync(IssueFirstTimeLicenseDto dto, CancellationToken cancellationToken = default);
    Task<LicenseResponseDto> RenewLicenseAsync(RenewLicenseDto dto, CancellationToken cancellationToken = default);
    Task<LicenseResponseDto> ReplaceLostLicenseAsync(ReplaceLostLicenseDto dto, CancellationToken cancellationToken = default);
    Task<LicenseResponseDto> ReplaceDamagedLicenseAsync(ReplaceDamagedLicenseDto dto, CancellationToken cancellationToken = default);
    Task<DetainedLicenseResponseDto> DetainLicenseAsync(DetainLicenseDto dto, CancellationToken cancellationToken = default);
    Task<DetainedLicenseResponseDto> ReleaseDetainedLicenseAsync(ReleaseLicenseDto dto, CancellationToken cancellationToken = default);
    Task<InternationalLicenseResponseDto> IssueInternationalLicenseAsync(IssueInternationalLicenseDto dto, CancellationToken cancellationToken = default);
    Task<LicenseResponseDto> GetLicenseByIdAsync(int licenseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LicenseResponseDto>> GetLicensesByDriverIdAsync(int driverId, CancellationToken cancellationToken = default);
}
