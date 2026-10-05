using DVLD.Domain.Enums;

namespace DVLD.Application.DTOs;

public record CreatePersonDto(
    string NationalNo,
    string FirstName,
    string SecondName,
    string? ThirdName,
    string LastName,
    DateTime DateOfBirth,
    EnGender Gender,
    string Address,
    string Phone,
    string Email,
    int NationalityCountryId,
    string? ImagePath
);

public record UpdatePersonDto(
    string FirstName,
    string SecondName,
    string? ThirdName,
    string LastName,
    DateTime DateOfBirth,
    EnGender Gender,
    string Address,
    string Phone,
    string Email,
    int NationalityCountryId,
    string? ImagePath
);

public record PersonResponseDto(
    int PersonId,
    string NationalNo,
    string FirstName,
    string SecondName,
    string? ThirdName,
    string LastName,
    string FullName,
    DateTime DateOfBirth,
    int Age,
    string Gender,
    string Address,
    string Phone,
    string Email,
    int NationalityCountryId,
    string? CountryName,
    string? ImagePath
);
