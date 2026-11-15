using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Employees;

internal static class EmployeeReader
{
    /// <summary>
    /// Loads an employee with every detail collection and lookup name in one round of queries.
    /// The context uses split queries by default, which avoids a cartesian explosion across the four collections.
    /// </summary>
    public static async Task<EmployeeDto> GetByIdAsync(IAppDbContext db, int id, CancellationToken ct)
    {
        var employee = await db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.EmployeeType)
            .Include(e => e.ReportingManager)
            .Include(e => e.Families).ThenInclude(f => f.Relationship)
            .Include(e => e.Addresses).ThenInclude(a => a.Country)
            .Include(e => e.Addresses).ThenInclude(a => a.State)
            .Include(e => e.Addresses).ThenInclude(a => a.City)
            .Include(e => e.Educations)
            .Include(e => e.Experiences)
            .FirstOrDefaultAsync(e => e.Id == id, ct)
            ?? throw new NotFoundException(nameof(Employee), id);

        return EmployeeMapper.ToDto(employee);
    }
}
