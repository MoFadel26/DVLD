using DVLD.Application.Common.Interfaces;
using DVLD.Application.DTOs;
using DVLD.Domain.Entities;
using DVLD.Domain.Exceptions;

namespace DVLD.Application.Services;

public class PersonService : IPersonService
{
    private readonly IUnitOfWork _unitOfWork;

    public PersonService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PersonResponseDto> CreatePersonAsync(CreatePersonDto dto, CancellationToken cancellationToken = default)
    {
        bool exists = await _unitOfWork.People.ExistsByNationalNoAsync(dto.NationalNo, cancellationToken);
        if (exists)
        {
            throw new DomainException($"A person with National ID '{dto.NationalNo}' already exists in the system.");
        }

        var person = new Person
        {
            NationalNo = dto.NationalNo.Trim(),
            FirstName = dto.FirstName.Trim(),
            SecondName = dto.SecondName.Trim(),
            ThirdName = dto.ThirdName?.Trim(),
            LastName = dto.LastName.Trim(),
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            Address = dto.Address.Trim(),
            Phone = dto.Phone.Trim(),
            Email = dto.Email.Trim(),
            NationalityCountryId = dto.NationalityCountryId,
            ImagePath = dto.ImagePath
        };

        await _unitOfWork.People.AddAsync(person, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDto(person, cancellationToken);
    }

    public async Task<PersonResponseDto> UpdatePersonAsync(int personId, UpdatePersonDto dto, CancellationToken cancellationToken = default)
    {
        var person = await _unitOfWork.People.GetByIdAsync(personId, cancellationToken)
            ?? throw new EntityNotFoundException("Person", personId);

        person.FirstName = dto.FirstName.Trim();
        person.SecondName = dto.SecondName.Trim();
        person.ThirdName = dto.ThirdName?.Trim();
        person.LastName = dto.LastName.Trim();
        person.DateOfBirth = dto.DateOfBirth;
        person.Gender = dto.Gender;
        person.Address = dto.Address.Trim();
        person.Phone = dto.Phone.Trim();
        person.Email = dto.Email.Trim();
        person.NationalityCountryId = dto.NationalityCountryId;
        person.ImagePath = dto.ImagePath;

        _unitOfWork.People.Update(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await MapToDto(person, cancellationToken);
    }

    public async Task<PersonResponseDto> GetByIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        var person = await _unitOfWork.People.GetByIdAsync(personId, cancellationToken)
            ?? throw new EntityNotFoundException("Person", personId);

        return await MapToDto(person, cancellationToken);
    }

    public async Task<PersonResponseDto> GetByNationalNoAsync(string nationalNo, CancellationToken cancellationToken = default)
    {
        var person = await _unitOfWork.People.GetByNationalNoAsync(nationalNo, cancellationToken)
            ?? throw new EntityNotFoundException("Person with National ID", nationalNo);

        return await MapToDto(person, cancellationToken);
    }

    public async Task<IReadOnlyList<PersonResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var people = await _unitOfWork.People.GetAllAsync(cancellationToken);
        var list = new List<PersonResponseDto>(people.Count);

        foreach (var p in people)
        {
            list.Add(await MapToDto(p, cancellationToken));
        }

        return list;
    }

    public async Task DeletePersonAsync(int personId, CancellationToken cancellationToken = default)
    {
        var person = await _unitOfWork.People.GetByIdAsync(personId, cancellationToken)
            ?? throw new EntityNotFoundException("Person", personId);

        _unitOfWork.People.Delete(person);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private Task<PersonResponseDto> MapToDto(Person person, CancellationToken cancellationToken)
    {
        var dto = new PersonResponseDto(
            person.PersonId,
            person.NationalNo,
            person.FirstName,
            person.SecondName,
            person.ThirdName,
            person.LastName,
            person.FullName,
            person.DateOfBirth,
            person.GetAge(),
            person.Gender.ToString(),
            person.Address,
            person.Phone,
            person.Email,
            person.NationalityCountryId,
            person.Country?.CountryName,
            person.ImagePath
        );

        return Task.FromResult(dto);
    }
}
