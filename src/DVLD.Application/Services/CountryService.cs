using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public class CountryService : ICountryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CountryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CountryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var countries = await _unitOfWork.Countries.GetAllAsync(cancellationToken);
        return countries.Select(c => new CountryDto(c.CountryId, c.CountryName)).ToList();
    }
}
