using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Features.Employees;

// ---------------------------------------------------------------- requests

/// <summary>
/// Detail inputs: <c>Id == null</c> adds a record, <c>Id</c> set updates it, and
/// existing records missing from the list are removed (update only).
/// </summary>
public sealed record FamilyInput
{
    public int? Id { get; init; }
    public int RelationshipId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Occupation { get; init; }
    public string? Phone { get; init; }
    public bool IsDependent { get; init; }
    public bool IsEmergencyContact { get; init; }
}

public sealed record AddressInput
{
    public int? Id { get; init; }
    public AddressType AddressType { get; init; }
    public string Line1 { get; init; } = string.Empty;
    public string? Line2 { get; init; }
    public int CountryId { get; init; }
    public int StateId { get; init; }
    public int CityId { get; init; }
    public string PostalCode { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed record EducationInput
{
    public int? Id { get; init; }
    public string Degree { get; init; } = string.Empty;
    public string? FieldOfStudy { get; init; }
    public string Institution { get; init; } = string.Empty;
    public int YearOfPassing { get; init; }
    public decimal? Percentage { get; init; }
}

public sealed record ExperienceInput
{
    public int? Id { get; init; }
    public string CompanyName { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? Responsibilities { get; init; }
    public string? ReasonForLeaving { get; init; }
}

/// <summary>Create and update payload (header + all detail collections).</summary>
public sealed record EmployeeRequest
{
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Mobile { get; init; } = string.Empty;
    public DateOnly JoiningDate { get; init; }
    public int DepartmentId { get; init; }
    public int DesignationId { get; init; }
    public int EmployeeTypeId { get; init; }
    public int? ReportingManagerId { get; init; }

    /// <summary>Required on update (optimistic concurrency); ignored on create.</summary>
    public string? RowVersion { get; init; }

    public List<FamilyInput> Families { get; init; } = [];
    public List<AddressInput> Addresses { get; init; } = [];
    public List<EducationInput> Educations { get; init; } = [];
    public List<ExperienceInput> Experiences { get; init; } = [];
}

// ---------------------------------------------------------------- responses

public sealed record FamilyDto(int Id, int RelationshipId, string Relationship, string FullName, DateOnly? DateOfBirth,
    string? Occupation, string? Phone, bool IsDependent, bool IsEmergencyContact);

public sealed record AddressDto(int Id, AddressType AddressType, string Line1, string? Line2, int CountryId, string Country,
    int StateId, string State, int CityId, string City, string PostalCode, bool IsPrimary);

public sealed record EducationDto(int Id, string Degree, string? FieldOfStudy, string Institution, int YearOfPassing, decimal? Percentage);

public sealed record ExperienceDto(int Id, string CompanyName, string JobTitle, DateOnly StartDate, DateOnly? EndDate,
    string? Responsibilities, string? ReasonForLeaving);

public sealed record EmployeeDto(
    int Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    string FullName,
    DateOnly DateOfBirth,
    Gender Gender,
    string Email,
    string Mobile,
    DateOnly JoiningDate,
    int DepartmentId,
    string Department,
    int DesignationId,
    string Designation,
    int EmployeeTypeId,
    string EmployeeType,
    int? ReportingManagerId,
    string? ReportingManager,
    bool IsActive,
    string RowVersion,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy,
    IReadOnlyList<FamilyDto> Families,
    IReadOnlyList<AddressDto> Addresses,
    IReadOnlyList<EducationDto> Educations,
    IReadOnlyList<ExperienceDto> Experiences);

/// <summary>Listing item. Intentionally excludes sensitive personal fields (DOB, mobile).</summary>
public sealed record EmployeeListItemDto(
    int Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string Department,
    string Designation,
    string EmployeeType,
    DateOnly JoiningDate,
    bool IsActive);
