using DVLD.Domain.Enums;

namespace DVLD.Domain.Entities;

public class Person
{
    public int PersonId { get; set; }
    public string NationalNo { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string? ThirdName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public EnGender Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int NationalityCountryId { get; set; }
    public string? ImagePath { get; set; }

    // Navigation
    public Country? Country { get; set; }
    public ICollection<Application> Applications { get; set; } = new List<Application>();

    public string FullName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ThirdName))
            {
                return $"{FirstName} {SecondName} {LastName}".Trim();
            }
            return $"{FirstName} {SecondName} {ThirdName} {LastName}".Trim();
        }
    }

    public int GetAge(DateTime? asOfDate = null)
    {
        DateTime referenceDate = asOfDate ?? DateTime.UtcNow;
        int age = referenceDate.Year - DateOfBirth.Year;
        if (referenceDate.Date < DateOfBirth.Date.AddYears(age))
        {
            age--;
        }
        return age;
    }
}
