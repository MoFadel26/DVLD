using DVLD.Application.DTOs;

namespace DVLD.Application.Services;

public interface IPersonService
{
    Task<PersonResponseDto> CreatePersonAsync(CreatePersonDto dto, CancellationToken cancellationToken = default);
    Task<PersonResponseDto> UpdatePersonAsync(int personId, UpdatePersonDto dto, CancellationToken cancellationToken = default);
    Task<PersonResponseDto> GetByIdAsync(int personId, CancellationToken cancellationToken = default);
    Task<PersonResponseDto> GetByNationalNoAsync(string nationalNo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task DeletePersonAsync(int personId, CancellationToken cancellationToken = default);
}
