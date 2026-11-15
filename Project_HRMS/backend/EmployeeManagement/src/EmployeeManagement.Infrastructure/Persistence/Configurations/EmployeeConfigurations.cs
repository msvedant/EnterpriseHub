using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeeManagement.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.ConfigureBase();

        builder.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(20);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.DateOfBirth).HasColumnType("date");
        builder.Property(e => e.JoiningDate).HasColumnType("date");
        builder.Property(e => e.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.Mobile).IsRequired().HasMaxLength(20);

        builder.HasOne(e => e.Department).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Designation).WithMany().HasForeignKey(e => e.DesignationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EmployeeType).WithMany().HasForeignKey(e => e.EmployeeTypeId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReportingManager).WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ReportingManagerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.EmployeeCode).IsUnique().HasFilter(BaseEntityConfiguration.ActiveFilter);
        builder.HasIndex(e => e.Email).IsUnique().HasFilter(BaseEntityConfiguration.ActiveFilter);
        builder.HasIndex(e => new { e.LastName, e.FirstName });
        builder.HasIndex(e => e.DepartmentId);
        builder.HasIndex(e => e.DesignationId);
        builder.HasIndex(e => e.EmployeeTypeId);
        builder.HasIndex(e => e.ReportingManagerId);
        builder.HasIndex(e => e.IsActive);
    }
}

internal sealed class EmployeeFamilyConfiguration : IEntityTypeConfiguration<EmployeeFamily>
{
    public void Configure(EntityTypeBuilder<EmployeeFamily> builder)
    {
        builder.ToTable("EmployeeFamilies");
        builder.ConfigureBase();

        builder.Property(f => f.FullName).IsRequired().HasMaxLength(150);
        builder.Property(f => f.DateOfBirth).HasColumnType("date");
        builder.Property(f => f.Occupation).HasMaxLength(100);
        builder.Property(f => f.Phone).HasMaxLength(20);

        builder.HasOne(f => f.Employee).WithMany(e => e.Families).HasForeignKey(f => f.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.Relationship).WithMany().HasForeignKey(f => f.RelationshipId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.EmployeeId);
        builder.HasIndex(f => f.RelationshipId);
    }
}

internal sealed class EmployeeAddressConfiguration : IEntityTypeConfiguration<EmployeeAddress>
{
    public void Configure(EntityTypeBuilder<EmployeeAddress> builder)
    {
        builder.ToTable("EmployeeAddresses");
        builder.ConfigureBase();

        builder.Property(a => a.AddressType).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Line1).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Line2).HasMaxLength(200);
        builder.Property(a => a.PostalCode).IsRequired().HasMaxLength(12);

        builder.HasOne(a => a.Employee).WithMany(e => e.Addresses).HasForeignKey(a => a.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(a => a.Country).WithMany().HasForeignKey(a => a.CountryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.State).WithMany().HasForeignKey(a => a.StateId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.City).WithMany().HasForeignKey(a => a.CityId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.EmployeeId);
        builder.HasIndex(a => a.CountryId);
        builder.HasIndex(a => a.StateId);
        builder.HasIndex(a => a.CityId);
    }
}

internal sealed class EmployeeEducationConfiguration : IEntityTypeConfiguration<EmployeeEducation>
{
    public void Configure(EntityTypeBuilder<EmployeeEducation> builder)
    {
        builder.ToTable("EmployeeEducations");
        builder.ConfigureBase();

        builder.Property(x => x.Degree).IsRequired().HasMaxLength(150);
        builder.Property(x => x.FieldOfStudy).HasMaxLength(150);
        builder.Property(x => x.Institution).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Percentage).HasPrecision(5, 2);

        builder.HasOne(x => x.Employee).WithMany(e => e.Educations).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.EmployeeId);
    }
}

internal sealed class EmployeeExperienceConfiguration : IEntityTypeConfiguration<EmployeeExperience>
{
    public void Configure(EntityTypeBuilder<EmployeeExperience> builder)
    {
        builder.ToTable("EmployeeExperiences");
        builder.ConfigureBase();

        builder.Property(x => x.CompanyName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.JobTitle).IsRequired().HasMaxLength(150);
        builder.Property(x => x.StartDate).HasColumnType("date");
        builder.Property(x => x.EndDate).HasColumnType("date");
        builder.Property(x => x.Responsibilities).HasMaxLength(1000);
        builder.Property(x => x.ReasonForLeaving).HasMaxLength(500);

        builder.HasOne(x => x.Employee).WithMany(e => e.Experiences).HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.EmployeeId);
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigureBase();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => u.Email).IsUnique().HasFilter(BaseEntityConfiguration.ActiveFilter);
    }
}
