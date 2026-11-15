using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Features.Employees;

internal static class EmployeeMapper
{
    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static void ApplyHeader(Employee e, EmployeeRequest r)
    {
        e.EmployeeCode = NormalizeCode(r.EmployeeCode);
        e.FirstName = r.FirstName.Trim();
        e.LastName = r.LastName.Trim();
        e.DateOfBirth = r.DateOfBirth;
        e.Gender = r.Gender;
        e.Email = NormalizeEmail(r.Email);
        e.Mobile = r.Mobile.Trim();
        e.JoiningDate = r.JoiningDate;
        e.DepartmentId = r.DepartmentId;
        e.DesignationId = r.DesignationId;
        e.EmployeeTypeId = r.EmployeeTypeId;
        e.ReportingManagerId = r.ReportingManagerId;
    }

    public static void Apply(EmployeeFamily e, FamilyInput i)
    {
        e.RelationshipId = i.RelationshipId;
        e.FullName = i.FullName.Trim();
        e.DateOfBirth = i.DateOfBirth;
        e.Occupation = Clean(i.Occupation);
        e.Phone = Clean(i.Phone);
        e.IsDependent = i.IsDependent;
        e.IsEmergencyContact = i.IsEmergencyContact;
    }

    public static void Apply(EmployeeAddress e, AddressInput i)
    {
        e.AddressType = i.AddressType;
        e.Line1 = i.Line1.Trim();
        e.Line2 = Clean(i.Line2);
        e.CountryId = i.CountryId;
        e.StateId = i.StateId;
        e.CityId = i.CityId;
        e.PostalCode = i.PostalCode.Trim();
        e.IsPrimary = i.IsPrimary;
    }

    public static void Apply(EmployeeEducation e, EducationInput i)
    {
        e.Degree = i.Degree.Trim();
        e.FieldOfStudy = Clean(i.FieldOfStudy);
        e.Institution = i.Institution.Trim();
        e.YearOfPassing = i.YearOfPassing;
        e.Percentage = i.Percentage;
    }

    public static void Apply(EmployeeExperience e, ExperienceInput i)
    {
        e.CompanyName = i.CompanyName.Trim();
        e.JobTitle = i.JobTitle.Trim();
        e.StartDate = i.StartDate;
        e.EndDate = i.EndDate;
        e.Responsibilities = Clean(i.Responsibilities);
        e.ReasonForLeaving = Clean(i.ReasonForLeaving);
    }

    public static EmployeeDto ToDto(Employee e) => new(
        e.Id,
        e.EmployeeCode,
        e.FirstName,
        e.LastName,
        $"{e.FirstName} {e.LastName}",
        e.DateOfBirth,
        e.Gender,
        e.Email,
        e.Mobile,
        e.JoiningDate,
        e.DepartmentId,
        e.Department?.Name ?? string.Empty,
        e.DesignationId,
        e.Designation?.Name ?? string.Empty,
        e.EmployeeTypeId,
        e.EmployeeType?.Name ?? string.Empty,
        e.ReportingManagerId,
        e.ReportingManager is null ? null : $"{e.ReportingManager.FirstName} {e.ReportingManager.LastName}",
        e.IsActive,
        Convert.ToBase64String(e.RowVersion),
        e.CreatedAt,
        e.CreatedBy,
        e.UpdatedAt,
        e.UpdatedBy,
        e.Families.OrderBy(f => f.Id).Select(f => new FamilyDto(f.Id, f.RelationshipId, f.Relationship?.Name ?? string.Empty,
            f.FullName, f.DateOfBirth, f.Occupation, f.Phone, f.IsDependent, f.IsEmergencyContact)).ToList(),
        e.Addresses.OrderBy(a => a.Id).Select(a => new AddressDto(a.Id, a.AddressType, a.Line1, a.Line2,
            a.CountryId, a.Country?.Name ?? string.Empty, a.StateId, a.State?.Name ?? string.Empty,
            a.CityId, a.City?.Name ?? string.Empty, a.PostalCode, a.IsPrimary)).ToList(),
        e.Educations.OrderBy(x => x.Id).Select(x => new EducationDto(x.Id, x.Degree, x.FieldOfStudy, x.Institution,
            x.YearOfPassing, x.Percentage)).ToList(),
        e.Experiences.OrderBy(x => x.Id).Select(x => new ExperienceDto(x.Id, x.CompanyName, x.JobTitle, x.StartDate,
            x.EndDate, x.Responsibilities, x.ReasonForLeaving)).ToList());
}
