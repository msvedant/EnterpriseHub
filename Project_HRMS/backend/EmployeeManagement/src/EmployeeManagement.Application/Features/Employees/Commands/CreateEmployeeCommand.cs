using EmployeeManagement.Application.Common.Abstractions;
using MediatR;

namespace EmployeeManagement.Application.Features.Employees.Commands;

public sealed record CreateEmployeeCommand(EmployeeRequest Data) : IRequest<EmployeeDto>;

public sealed class CreateEmployeeCommandHandler(IAppDbContext db) : IRequestHandler<CreateEmployeeCommand, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var r = request.Data;

        // Header and all detail rows are written in one transaction.
        var id = await db.ExecuteInTransactionAsync(async ct =>
        {
            await EmployeeRules.EnsureValidAsync(db, r, null, ct);

            var employee = new Domain.Entities.Employee();
            EmployeeMapper.ApplyHeader(employee, r);

            foreach (var input in r.Families)
            {
                var item = new Domain.Entities.EmployeeFamily();
                EmployeeMapper.Apply(item, input);
                employee.Families.Add(item);
            }

            foreach (var input in r.Addresses)
            {
                var item = new Domain.Entities.EmployeeAddress();
                EmployeeMapper.Apply(item, input);
                employee.Addresses.Add(item);
            }

            foreach (var input in r.Educations)
            {
                var item = new Domain.Entities.EmployeeEducation();
                EmployeeMapper.Apply(item, input);
                employee.Educations.Add(item);
            }

            foreach (var input in r.Experiences)
            {
                var item = new Domain.Entities.EmployeeExperience();
                EmployeeMapper.Apply(item, input);
                employee.Experiences.Add(item);
            }

            db.Employees.Add(employee);
            await db.SaveChangesAsync(ct);
            return employee.Id;
        }, cancellationToken);

        return await EmployeeReader.GetByIdAsync(db, id, cancellationToken);
    }
}
