using DVLD.Application.Common.Interfaces;
using DVLD.Domain.Entities;
using DVLD.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DVLD.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(DvldDbContext context, ILogger logger, IPasswordHasher passwordHasher, string? adminPassword)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();

            // Seed the admin account if there are no users
            if (!await context.Users.AnyAsync())
            {
                if (string.IsNullOrWhiteSpace(adminPassword))
                {
                    logger.LogWarning("No users exist and Auth:SeedAdminPassword is not set, so nobody can sign in.");
                }
                else
                {
                    logger.LogInformation("Seeding the admin user...");
                    context.Users.Add(new User
                    {
                        Username = "admin",
                        PasswordHash = passwordHasher.Hash(adminPassword)
                    });
                    await context.SaveChangesAsync();
                }
            }

            // Seed Countries if empty
            if (!await context.Countries.AnyAsync())
            {
                logger.LogInformation("Seeding default countries...");
                var countries = new List<Country>
                {
                    new() { CountryName = "Jordan" },
                    new() { CountryName = "United States" },
                    new() { CountryName = "United Kingdom" },
                    new() { CountryName = "Canada" },
                    new() { CountryName = "Germany" },
                    new() { CountryName = "Saudi Arabia" },
                    new() { CountryName = "United Arab Emirates" },
                    new() { CountryName = "Egypt" }
                };

                await context.Countries.AddRangeAsync(countries);
                await context.SaveChangesAsync();
            }

            // Seed sample applicant persons if empty
            if (!await context.People.AnyAsync())
            {
                logger.LogInformation("Seeding initial demo applicants...");
                var jordan = await context.Countries.FirstAsync(c => c.CountryName == "Jordan");

                var samplePeople = new List<Person>
                {
                    new()
                    {
                        NationalNo = "N1001",
                        FirstName = "Ahmad",
                        SecondName = "Mohammad",
                        ThirdName = "Ali",
                        LastName = "Al-Sayed",
                        DateOfBirth = DateTime.UtcNow.AddYears(-25), // 25 years old (Eligible for all categories)
                        Gender = EnGender.Male,
                        Address = "Queen Rania Street, Building 45",
                        Phone = "+962791112233",
                        Email = "ahmad.alsayed@example.com",
                        NationalityCountryId = jordan.CountryId
                    },
                    new()
                    {
                        NationalNo = "N1002",
                        FirstName = "Sara",
                        SecondName = "Omar",
                        ThirdName = "Khalid",
                        LastName = "Mansoor",
                        DateOfBirth = DateTime.UtcNow.AddYears(-16), // 16 years old (Underage for all driving licenses)
                        Gender = EnGender.Female,
                        Address = "Airport Road, Villa 12",
                        Phone = "+962795556677",
                        Email = "sara.mansoor@example.com",
                        NationalityCountryId = jordan.CountryId
                    },
                    new()
                    {
                        NationalNo = "N1003",
                        FirstName = "Rami",
                        SecondName = "Saeed",
                        ThirdName = "Hassan",
                        LastName = "Najjar",
                        DateOfBirth = DateTime.UtcNow.AddYears(-19), // 19 years old (Eligible for Class 1 & 3, not Class 2, 4, 5, 6, 7)
                        Gender = EnGender.Male,
                        Address = "Mecca Street, Complex 8",
                        Phone = "+962798889900",
                        Email = "rami.najjar@example.com",
                        NationalityCountryId = jordan.CountryId
                    }
                };

                await context.People.AddRangeAsync(samplePeople);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the DVLD database.");
        }
    }
}
