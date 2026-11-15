using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Features.Employees;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EmployeeManagement.UnitTests.Helpers;

public sealed record LookupIds(int DepartmentId, int DesignationId, int EmployeeTypeId, int RelationshipId,
    int CountryId, int StateId, int CityId);

public static class TestDb
{
    public static AppDbContext Create(string? name = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .Options;

        var user = new Mock<ICurrentUserService>();
        user.SetupGet(u => u.UserName).Returns("tester@example.com");
        return new AppDbContext(options, user.Object, TimeProvider.System);
    }

    public static async Task<LookupIds> SeedLookupsAsync(AppDbContext db)
    {
        var department = new Department { Name = "Engineering", Code = "ENG" };
        var designation = new Designation { Name = "Software Engineer" };
        var type = new EmployeeType { Name = "Full-Time" };
        var relationship = new Relationship { Name = "Spouse" };
        var country = new Country { Name = "India", Code = "IN" };
        var state = new State { Name = "Maharashtra", Country = country };
        var city = new City { Name = "Pune", State = state };

        db.AddRange(department, designation, type, relationship, country, state, city);
        await db.SaveChangesAsync();

        return new LookupIds(department.Id, designation.Id, type.Id, relationship.Id, country.Id, state.Id, city.Id);
    }

    public static EmployeeRequest ValidRequest(LookupIds ids, string code = "EMP001", string email = "john.doe@example.com") => new()
    {
        EmployeeCode = code,
        FirstName = "John",
        LastName = "Doe",
        DateOfBirth = new DateOnly(1990, 5, 20),
        Gender = Gender.Male,
        Email = email,
        Mobile = "+919876543210",
        JoiningDate = new DateOnly(2022, 1, 10),
        DepartmentId = ids.DepartmentId,
        DesignationId = ids.DesignationId,
        EmployeeTypeId = ids.EmployeeTypeId,
        Families =
        [
            new FamilyInput { RelationshipId = ids.RelationshipId, FullName = "Jane Doe", IsDependent = true, IsEmergencyContact = true }
        ],
        Addresses =
        [
            new AddressInput
            {
                AddressType = AddressType.Permanent, Line1 = "12 Main Street", CountryId = ids.CountryId,
                StateId = ids.StateId, CityId = ids.CityId, PostalCode = "411001", IsPrimary = true
            }
        ],
        Educations =
        [
            new EducationInput { Degree = "B.Tech", Institution = "State University", YearOfPassing = 2012, Percentage = 78.5m }
        ],
        Experiences =
        [
            new ExperienceInput
            {
                CompanyName = "Acme Corp", JobTitle = "Developer",
                StartDate = new DateOnly(2012, 7, 1), EndDate = new DateOnly(2021, 12, 31)
            }
        ]
    };
}
