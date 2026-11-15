using EmployeeManagement.Application.Common;
using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Employees.Commands;

public sealed record UpdateEmployeeCommand(int Id, EmployeeRequest Data) : IRequest<EmployeeDto>;

public sealed class UpdateEmployeeCommandHandler(IAppDbContext db) : IRequestHandler<UpdateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var r = request.Data;

        await db.ExecuteInTransactionAsync(async ct =>
        {
            var employee = await db.Employees
                .Include(e => e.Families)
                .Include(e => e.Addresses)
                .Include(e => e.Educations)
                .Include(e => e.Experiences)
                .FirstOrDefaultAsync(e => e.Id == request.Id, ct)
                ?? throw new NotFoundException(nameof(Employee), request.Id);

            await EmployeeRules.EnsureValidAsync(db, r, employee.Id, ct);

            ConcurrencyHelper.ApplyRowVersion(db, employee, r.RowVersion);
            EmployeeMapper.ApplyHeader(employee, r);

            Sync(db, employee.Families, r.Families, i => i.Id, EmployeeMapper.Apply, "families");
            Sync(db, employee.Addresses, r.Addresses, i => i.Id, EmployeeMapper.Apply, "addresses");
            Sync(db, employee.Educations, r.Educations, i => i.Id, EmployeeMapper.Apply, "educations");
            Sync(db, employee.Experiences, r.Experiences, i => i.Id, EmployeeMapper.Apply, "experiences");

            // Always touch the header so the row-version check runs even when only details changed.
            db.Entry(employee).Property(e => e.UpdatedAt).IsModified = true;

            await db.SaveChangesAsync(ct);
            return employee.Id;
        }, cancellationToken);

        return await EmployeeReader.GetByIdAsync(db, request.Id, cancellationToken);
    }

    /// <summary>Adds, updates and removes detail rows so the stored collection matches the request.</summary>
    private static void Sync<TEntity, TInput>(
        IAppDbContext db,
        List<TEntity> existing,
        IEnumerable<TInput> inputs,
        Func<TInput, int?> getId,
        Action<TEntity, TInput> apply,
        string label)
        where TEntity : BaseEntity, new()
    {
        var list = inputs.ToList();
        var ids = list.Select(getId).Where(id => id.HasValue).Select(id => id!.Value).ToList();

        if (ids.Count != ids.Distinct().Count())
            throw Fail.Validation(label, "Duplicate record ids were supplied.");

        var current = existing.ToDictionary(x => x.Id);
        var unknown = ids.Where(id => !current.ContainsKey(id)).ToList();
        if (unknown.Count > 0)
            throw Fail.Validation(label, $"Record id(s) {string.Join(", ", unknown)} do not belong to this employee.");

        foreach (var removed in existing.Where(x => !ids.Contains(x.Id)).ToList())
        {
            existing.Remove(removed);
            db.Set<TEntity>().Remove(removed);
        }

        foreach (var input in list)
        {
            if (getId(input) is { } id)
            {
                apply(current[id], input);
            }
            else
            {
                var added = new TEntity();
                apply(added, input);
                existing.Add(added);
            }
        }
    }
}
