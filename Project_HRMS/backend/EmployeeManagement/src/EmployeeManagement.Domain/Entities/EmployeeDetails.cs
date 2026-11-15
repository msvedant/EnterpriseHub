using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Entities;

public sealed class EmployeeFamily : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int RelationshipId { get; set; }
    public Relationship Relationship { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Occupation { get; set; }
    public string? Phone { get; set; }
    public bool IsDependent { get; set; }
    public bool IsEmergencyContact { get; set; }
}

public sealed class EmployeeAddress : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public AddressType AddressType { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }

    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;

    public int StateId { get; set; }
    public State State { get; set; } = null!;

    public int CityId { get; set; }
    public City City { get; set; } = null!;

    public string PostalCode { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public sealed class EmployeeEducation : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string Degree { get; set; } = string.Empty;
    public string? FieldOfStudy { get; set; }
    public string Institution { get; set; } = string.Empty;
    public int YearOfPassing { get; set; }
    public decimal? Percentage { get; set; }
}

public sealed class EmployeeExperience : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string CompanyName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Responsibilities { get; set; }
    public string? ReasonForLeaving { get; set; }
}
