using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public interface ICountryService
{
    Task<IReadOnlyList<CountryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
