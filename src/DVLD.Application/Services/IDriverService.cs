using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public interface IDriverService
{
    Task<IReadOnlyList<DriverDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<DriverDto> GetByIdAsync(int driverId, CancellationToken cancellationToken = default);
}
