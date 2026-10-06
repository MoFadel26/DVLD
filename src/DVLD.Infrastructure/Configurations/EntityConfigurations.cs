using DVLD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DVLD.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).ValueGeneratedOnAdd();
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(200);
    }
}

public class RevokedTokenConfiguration : IEntityTypeConfiguration<RevokedToken>
{
    public void Configure(EntityTypeBuilder<RevokedToken> builder)
    {
        builder.ToTable("RevokedTokens");
        builder.HasKey(t => t.RevokedTokenId);
        builder.Property(t => t.RevokedTokenId).ValueGeneratedOnAdd();
        builder.Property(t => t.TokenId).IsRequired().HasMaxLength(64);
        builder.HasIndex(t => t.TokenId).IsUnique();
    }
}

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(c => c.CountryId);
        builder.Property(c => c.CountryId).ValueGeneratedOnAdd();
        builder.Property(c => c.CountryName).IsRequired().HasMaxLength(100);
    }
}

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("People");
        builder.HasKey(p => p.PersonId);
        builder.Property(p => p.PersonId).ValueGeneratedOnAdd();

        builder.Property(p => p.NationalNo).IsRequired().HasMaxLength(50);
        builder.HasIndex(p => p.NationalNo).IsUnique();

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.SecondName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.ThirdName).HasMaxLength(50);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Address).IsRequired().HasMaxLength(250);
        builder.Property(p => p.Phone).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(100);
        builder.Property(p => p.ImagePath).HasMaxLength(255);

        builder.HasOne(p => p.Country)
            .WithMany()
            .HasForeignKey(p => p.NationalityCountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LicenseClassConfiguration : IEntityTypeConfiguration<LicenseClass>
{
    public void Configure(EntityTypeBuilder<LicenseClass> builder)
    {
        builder.ToTable("LicenseClasses");
        builder.HasKey(lc => lc.LicenseClassId);

        builder.Property(lc => lc.ClassName).IsRequired().HasMaxLength(100);
        builder.Property(lc => lc.ClassDescription).IsRequired().HasMaxLength(500);
        builder.Property(lc => lc.ClassFees).HasPrecision(18, 2);

        builder.HasData(
            new LicenseClass
            {
                LicenseClassId = 1,
                ClassName = "Class 1 - Small Motorcycle License",
                ClassDescription = "Allows the driver to ride motorcycles with small and low-power engine capacity.",
                MinimumAllowedAge = 18,
                ValidityLength = 5,
                ClassFees = 15.00m
            },
            new LicenseClass
            {
                LicenseClassId = 2,
                ClassName = "Class 2 - Heavy Motorcycle License (Large Motorcycle)",
                ClassDescription = "Allows the driver to operate large, powerful motorcycles.",
                MinimumAllowedAge = 21,
                ValidityLength = 5,
                ClassFees = 30.00m
            },
            new LicenseClass
            {
                LicenseClassId = 3,
                ClassName = "Class 3 - Standard Driving License (Car License)",
                ClassDescription = "Allows the driver to operate light vehicles and personal cars.",
                MinimumAllowedAge = 18,
                ValidityLength = 10,
                ClassFees = 20.00m
            },
            new LicenseClass
            {
                LicenseClassId = 4,
                ClassName = "Class 4 - Commercial Driving License (Taxi/Limousine)",
                ClassDescription = "Allows the driver to operate taxis or limousines.",
                MinimumAllowedAge = 21,
                ValidityLength = 10,
                ClassFees = 200.00m
            },
            new LicenseClass
            {
                LicenseClassId = 5,
                ClassName = "Class 5 - Agricultural Vehicle Driving License (Tractors/Tilling)",
                ClassDescription = "Allows the driver to operate all agricultural vehicles.",
                MinimumAllowedAge = 21,
                ValidityLength = 10,
                ClassFees = 50.00m
            },
            new LicenseClass
            {
                LicenseClassId = 6,
                ClassName = "Class 6 - Small and Medium Bus License",
                ClassDescription = "Allows the driver to operate small and medium-sized buses.",
                MinimumAllowedAge = 21,
                ValidityLength = 10,
                ClassFees = 250.00m
            },
            new LicenseClass
            {
                LicenseClassId = 7,
                ClassName = "Class 7 - Truck and Heavy Vehicle License",
                ClassDescription = "Allows the driver to operate trucks and heavy vehicles, such as buses and large trucks.",
                MinimumAllowedAge = 21,
                ValidityLength = 10,
                ClassFees = 300.00m
            }
        );
    }
}

public class ApplicationTypeConfiguration : IEntityTypeConfiguration<ApplicationType>
{
    public void Configure(EntityTypeBuilder<ApplicationType> builder)
    {
        builder.ToTable("ApplicationTypes");
        builder.HasKey(at => at.ApplicationTypeId);

        builder.Property(at => at.ApplicationTypeTitle).IsRequired().HasMaxLength(150);
        builder.Property(at => at.ApplicationFees).HasPrecision(18, 2);

        builder.HasData(
            new ApplicationType { ApplicationTypeId = 1, ApplicationTypeTitle = "New Driving License Service", ApplicationFees = 5.00m },
            new ApplicationType { ApplicationTypeId = 2, ApplicationTypeTitle = "Renew Driving License Service", ApplicationFees = 5.00m },
            new ApplicationType { ApplicationTypeId = 3, ApplicationTypeTitle = "Replacement for Lost License Service", ApplicationFees = 5.00m },
            new ApplicationType { ApplicationTypeId = 4, ApplicationTypeTitle = "Replacement for Damaged License Service", ApplicationFees = 5.00m },
            new ApplicationType { ApplicationTypeId = 5, ApplicationTypeTitle = "Release Detained Driving License Service", ApplicationFees = 5.00m },
            new ApplicationType { ApplicationTypeId = 6, ApplicationTypeTitle = "Re-examination (Retake Test) Service", ApplicationFees = 5.00m },
            new ApplicationType { ApplicationTypeId = 7, ApplicationTypeTitle = "Issue International License Service", ApplicationFees = 5.00m }
        );
    }
}

public class TestTypeConfiguration : IEntityTypeConfiguration<TestType>
{
    public void Configure(EntityTypeBuilder<TestType> builder)
    {
        builder.ToTable("TestTypes");
        builder.HasKey(tt => tt.TestTypeId);

        builder.Property(tt => tt.TestTypeTitle).IsRequired().HasMaxLength(100);
        builder.Property(tt => tt.TestTypeDescription).IsRequired().HasMaxLength(500);
        builder.Property(tt => tt.TestTypeFees).HasPrecision(18, 2);

        builder.HasData(
            new TestType
            {
                TestTypeId = 1,
                TestTypeTitle = "Vision Test",
                TestTypeDescription = "Verifies applicant's visual acuity and physical fitness to safely drive.",
                TestTypeFees = 10.00m
            },
            new TestType
            {
                TestTypeId = 2,
                TestTypeTitle = "Theory Test",
                TestTypeDescription = "Evaluates traffic laws, road signs, and safe driving protocols.",
                TestTypeFees = 20.00m
            },
            new TestType
            {
                TestTypeId = 3,
                TestTypeTitle = "Practical Test (Street Driving)",
                TestTypeDescription = "Assesses practical on-road vehicle handling and real traffic maneuvering.",
                TestTypeFees = 30.00m
            }
        );
    }
}

public class ApplicationConfiguration : IEntityTypeConfiguration<Domain.Entities.Application>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Application> builder)
    {
        builder.ToTable("Applications");
        builder.HasKey(a => a.ApplicationId);
        builder.Property(a => a.ApplicationId).ValueGeneratedOnAdd();

        builder.Property(a => a.PaidFees).HasPrecision(18, 2);

        builder.HasOne(a => a.Person)
            .WithMany(p => p.Applications)
            .HasForeignKey(a => a.ApplicantPersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ApplicationType)
            .WithMany()
            .HasForeignKey(a => a.ApplicationTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LocalDrivingLicenseApplicationConfiguration : IEntityTypeConfiguration<LocalDrivingLicenseApplication>
{
    public void Configure(EntityTypeBuilder<LocalDrivingLicenseApplication> builder)
    {
        builder.ToTable("LocalDrivingLicenseApplications");
        builder.HasKey(l => l.LocalDrivingLicenseApplicationId);
        builder.Property(l => l.LocalDrivingLicenseApplicationId).ValueGeneratedOnAdd();

        builder.HasOne(l => l.Application)
            .WithMany()
            .HasForeignKey(l => l.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.LicenseClass)
            .WithMany()
            .HasForeignKey(l => l.LicenseClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TestAppointmentConfiguration : IEntityTypeConfiguration<TestAppointment>
{
    public void Configure(EntityTypeBuilder<TestAppointment> builder)
    {
        builder.ToTable("TestAppointments");
        builder.HasKey(ta => ta.TestAppointmentId);
        builder.Property(ta => ta.TestAppointmentId).ValueGeneratedOnAdd();

        builder.Property(ta => ta.PaidFees).HasPrecision(18, 2);

        builder.HasOne(ta => ta.TestType)
            .WithMany()
            .HasForeignKey(ta => ta.TestTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ta => ta.LocalDrivingLicenseApplication)
            .WithMany(l => l.TestAppointments)
            .HasForeignKey(ta => ta.LocalDrivingLicenseApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TestResultRecordConfiguration : IEntityTypeConfiguration<TestResultRecord>
{
    public void Configure(EntityTypeBuilder<TestResultRecord> builder)
    {
        builder.ToTable("TestResults");
        builder.HasKey(tr => tr.TestId);
        builder.Property(tr => tr.TestId).ValueGeneratedOnAdd();

        builder.Property(tr => tr.Notes).HasMaxLength(500);

        builder.HasOne(tr => tr.TestAppointment)
            .WithOne(ta => ta.TestResultRecord)
            .HasForeignKey<TestResultRecord>(tr => tr.TestAppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");
        builder.HasKey(d => d.DriverId);
        builder.Property(d => d.DriverId).ValueGeneratedOnAdd();

        builder.HasOne(d => d.Person)
            .WithMany()
            .HasForeignKey(d => d.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.PersonId).IsUnique();
    }
}

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");
        builder.HasKey(l => l.LicenseId);
        builder.Property(l => l.LicenseId).ValueGeneratedOnAdd();

        builder.Property(l => l.PaidFees).HasPrecision(18, 2);
        builder.Property(l => l.Notes).HasMaxLength(500);

        builder.HasOne(l => l.Application)
            .WithMany()
            .HasForeignKey(l => l.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Driver)
            .WithMany(d => d.Licenses)
            .HasForeignKey(l => l.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.LicenseClass)
            .WithMany()
            .HasForeignKey(l => l.LicenseClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InternationalLicenseConfiguration : IEntityTypeConfiguration<InternationalLicense>
{
    public void Configure(EntityTypeBuilder<InternationalLicense> builder)
    {
        builder.ToTable("InternationalLicenses");
        builder.HasKey(il => il.InternationalLicenseId);
        builder.Property(il => il.InternationalLicenseId).ValueGeneratedOnAdd();

        builder.HasOne(il => il.Application)
            .WithMany()
            .HasForeignKey(il => il.ApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(il => il.Driver)
            .WithMany(d => d.InternationalLicenses)
            .HasForeignKey(il => il.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(il => il.IssuedUsingLocalLicense)
            .WithMany()
            .HasForeignKey(il => il.IssuedUsingLocalLicenseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DetainedLicenseConfiguration : IEntityTypeConfiguration<DetainedLicense>
{
    public void Configure(EntityTypeBuilder<DetainedLicense> builder)
    {
        builder.ToTable("DetainedLicenses");
        builder.HasKey(dl => dl.DetainId);
        builder.Property(dl => dl.DetainId).ValueGeneratedOnAdd();

        builder.Property(dl => dl.FineFees).HasPrecision(18, 2);

        builder.HasOne(dl => dl.License)
            .WithMany(l => l.DetainedLicenses)
            .HasForeignKey(dl => dl.LicenseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(dl => dl.ReleaseApplication)
            .WithMany()
            .HasForeignKey(dl => dl.ReleaseApplicationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
