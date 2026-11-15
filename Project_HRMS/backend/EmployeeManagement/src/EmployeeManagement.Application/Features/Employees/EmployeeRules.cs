using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Employees;

/// <summary>Database-dependent business rules shared by the create and update handlers.</summary>
internal static class EmployeeRules
{
    public static async Task EnsureValidAsync(IAppDbContext db, EmployeeRequest r, int? excludeId, CancellationToken ct)
    {
        var exclude = excludeId ?? 0;
        var code = EmployeeMapper.NormalizeCode(r.EmployeeCode);
        var email = EmployeeMapper.NormalizeEmail(r.Email);

        if (await db.Employees.AnyAsync(e => e.EmployeeCode == code && e.Id != exclude, ct))
            throw new ConflictException($"Employee code '{code}' is already in use.");

        if (await db.Employees.AnyAsync(e => e.Email == email && e.Id != exclude, ct))
            throw new ConflictException("An employee with this email address already exists.");

        if (!await db.Departments.AnyAsync(d => d.Id == r.DepartmentId && d.IsActive, ct))
            throw Fail.Validation("departmentId", "Department does not exist or is inactive.");

        if (!await db.Designations.AnyAsync(d => d.Id == r.DesignationId && d.IsActive, ct))
            throw Fail.Validation("designationId", "Designation does not exist or is inactive.");

        if (!await db.EmployeeTypes.AnyAsync(t => t.Id == r.EmployeeTypeId && t.IsActive, ct))
            throw Fail.Validation("employeeTypeId", "Employee type does not exist or is inactive.");

        if (r.ReportingManagerId is { } managerId)
            await EnsureManagerAsync(db, managerId, excludeId, ct);

        await EnsureRelationshipsAsync(db, r, ct);
        await EnsureAddressesAsync(db, r, ct);
    }

    private static async Task EnsureManagerAsync(IAppDbContext db, int managerId, int? employeeId, CancellationToken ct)
    {
        if (employeeId == managerId)
            throw Fail.Validation("reportingManagerId", "An employee cannot report to themselves.");

        var manager = await db.Employees.AsNoTracking()
            .Where(e => e.Id == managerId)
            .Select(e => new { e.IsActive, e.ReportingManagerId })
            .FirstOrDefaultAsync(ct);

        if (manager is null || !manager.IsActive)
            throw Fail.Validation("reportingManagerId", "Reporting manager does not exist or is inactive.");

        if (employeeId is not { } self)
            return;

        // Walk up the management chain to prevent circular reporting lines.
        var current = manager.ReportingManagerId;
        var depth = 0;
        while (current is { } next && depth++ < 100)
        {
            if (next == self)
                throw Fail.Validation("reportingManagerId", "This reporting manager would create a circular reporting line.");

            current = await db.Employees.AsNoTracking()
                .Where(e => e.Id == next)
                .Select(e => e.ReportingManagerId)
                .FirstOrDefaultAsync(ct);
        }
    }

    private static async Task EnsureRelationshipsAsync(IAppDbContext db, EmployeeRequest r, CancellationToken ct)
    {
        var ids = r.Families.Select(f => f.RelationshipId).Distinct().ToList();
        if (ids.Count == 0)
            return;

        var found = await db.Relationships.CountAsync(x => ids.Contains(x.Id) && x.IsActive, ct);
        if (found != ids.Count)
            throw Fail.Validation("families", "One or more relationships do not exist or are inactive.");
    }

    private static async Task EnsureAddressesAsync(IAppDbContext db, EmployeeRequest r, CancellationToken ct)
    {
        if (r.Addresses.Count == 0)
            return;

        var countryIds = r.Addresses.Select(a => a.CountryId).Distinct().ToList();
        var stateIds = r.Addresses.Select(a => a.StateId).Distinct().ToList();
        var cityIds = r.Addresses.Select(a => a.CityId).Distinct().ToList();

        var countries = await db.Countries.AsNoTracking()
            .Where(c => countryIds.Contains(c.Id) && c.IsActive).Select(c => c.Id).ToListAsync(ct);
        var states = await db.States.AsNoTracking()
            .Where(s => stateIds.Contains(s.Id) && s.IsActive).Select(s => new { s.Id, s.CountryId }).ToListAsync(ct);
        var cities = await db.Cities.AsNoTracking()
            .Where(c => cityIds.Contains(c.Id) && c.IsActive).Select(c => new { c.Id, c.StateId }).ToListAsync(ct);

        for (var i = 0; i < r.Addresses.Count; i++)
        {
            var a = r.Addresses[i];
            var path = $"addresses[{i}]";

            if (!countries.Contains(a.CountryId))
                throw Fail.Validation($"{path}.countryId", "Country does not exist or is inactive.");

            var state = states.FirstOrDefault(s => s.Id == a.StateId);
            if (state is null || state.CountryId != a.CountryId)
                throw Fail.Validation($"{path}.stateId", "State does not exist, is inactive, or does not belong to the country.");

            var city = cities.FirstOrDefault(c => c.Id == a.CityId);
            if (city is null || city.StateId != a.StateId)
                throw Fail.Validation($"{path}.cityId", "City does not exist, is inactive, or does not belong to the state.");
        }
    }
}
