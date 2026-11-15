using System.Linq.Expressions;
using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Employees.Queries;

public sealed record GetEmployeesQuery : IRequest<PagedResult<EmployeeListItemDto>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    /// <summary>employeeCode | firstName | lastName | email | joiningDate | department | designation | createdAt</summary>
    public string SortBy { get; init; } = "employeeCode";

    /// <summary>asc | desc</summary>
    public string SortDirection { get; init; } = "asc";

    public string? Search { get; init; }
    public int? DepartmentId { get; init; }
    public int? DesignationId { get; init; }
    public int? EmployeeTypeId { get; init; }
    public int? ReportingManagerId { get; init; }
    public Gender? Gender { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class GetEmployeesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetEmployeesQuery, PagedResult<EmployeeListItemDto>>
{
    /// <summary>Whitelist of sortable columns (prevents arbitrary column / injection style sorting).</summary>
    internal static readonly Dictionary<string, Expression<Func<Employee, object>>> SortColumns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["employeeCode"] = e => e.EmployeeCode,
            ["firstName"] = e => e.FirstName,
            ["lastName"] = e => e.LastName,
            ["email"] = e => e.Email,
            ["joiningDate"] = e => e.JoiningDate,
            ["department"] = e => e.Department.Name,
            ["designation"] = e => e.Designation.Name,
            ["createdAt"] = e => e.CreatedAt
        };

    public async Task<PagedResult<EmployeeListItemDto>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var size = Math.Clamp(request.PageSize, 1, 100);

        var query = db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(e =>
                e.EmployeeCode.Contains(term) ||
                e.FirstName.Contains(term) ||
                e.LastName.Contains(term) ||
                e.Email.Contains(term) ||
                (e.FirstName + " " + e.LastName).Contains(term));
        }

        if (request.DepartmentId is { } departmentId)
            query = query.Where(e => e.DepartmentId == departmentId);
        if (request.DesignationId is { } designationId)
            query = query.Where(e => e.DesignationId == designationId);
        if (request.EmployeeTypeId is { } typeId)
            query = query.Where(e => e.EmployeeTypeId == typeId);
        if (request.ReportingManagerId is { } managerId)
            query = query.Where(e => e.ReportingManagerId == managerId);
        if (request.Gender is { } gender)
            query = query.Where(e => e.Gender == gender);
        if (request.IsActive is { } isActive)
            query = query.Where(e => e.IsActive == isActive);

        var total = await query.CountAsync(cancellationToken);

        var sort = SortColumns.TryGetValue(request.SortBy, out var column) ? column : SortColumns["employeeCode"];
        var descending = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var ordered = descending ? query.OrderByDescending(sort) : query.OrderBy(sort);

        var items = await ordered
            .ThenBy(e => e.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(e => new EmployeeListItemDto(
                e.Id,
                e.EmployeeCode,
                e.FirstName + " " + e.LastName,
                e.Email,
                e.Department.Name,
                e.Designation.Name,
                e.EmployeeType.Name,
                e.JoiningDate,
                e.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<EmployeeListItemDto>(items, page, size, total);
    }
}
