namespace EmployeeManagement.Application.Features.Masters;

public enum MasterKind
{
    Department,
    Designation,
    EmployeeType,
    Relationship,
    Country,
    State,
    City
}

public sealed record MasterRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? Description { get; init; }

    /// <summary>Required for State (CountryId) and City (StateId); ignored for other masters.</summary>
    public int? ParentId { get; init; }

    /// <summary>Required on update (optimistic concurrency).</summary>
    public string? RowVersion { get; init; }
}

public sealed record MasterDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? Description { get; init; }
    public int? ParentId { get; init; }
    public bool IsActive { get; init; }
    public string RowVersion { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record MasterListFilter
{
    public string? Search { get; init; }
    public int? ParentId { get; init; }
    public bool? IsActive { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
