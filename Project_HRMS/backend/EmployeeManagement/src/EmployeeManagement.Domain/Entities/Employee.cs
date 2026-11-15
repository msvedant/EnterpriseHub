using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Entities;

/// <summary>Employee header record.</summary>
public sealed class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public DateOnly JoiningDate { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int DesignationId { get; set; }
    public Designation Designation { get; set; } = null!;

    public int EmployeeTypeId { get; set; }
    public EmployeeType EmployeeType { get; set; } = null!;

    public int? ReportingManagerId { get; set; }
    public Employee? ReportingManager { get; set; }
    public List<Employee> DirectReports { get; set; } = [];

    public List<EmployeeFamily> Families { get; set; } = [];
    public List<EmployeeAddress> Addresses { get; set; } = [];
    public List<EmployeeEducation> Educations { get; set; } = [];
    public List<EmployeeExperience> Experiences { get; set; } = [];
}
