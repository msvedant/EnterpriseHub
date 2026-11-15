using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Employees.Commands;

public sealed record SetEmployeeStatusCommand(int Id, bool IsActive) : IRequest;

public sealed record DeleteEmployeeCommand(int Id) : IRequest;

public sealed class SetEmployeeStatusCommandHandler(IAppDbContext db) : IRequestHandler<SetEmployeeStatusCommand>
{
    public async Task Handle(SetEmployeeStatusCommand request, CancellationToken cancellationToken)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Employee), request.Id);

        employee.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Soft delete: the row is kept for audit purposes and hidden by the global query filter.</summary>
public sealed class DeleteEmployeeCommandHandler(IAppDbContext db) : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                       ?? throw new NotFoundException(nameof(Employee), request.Id);

        if (await db.Employees.AnyAsync(e => e.ReportingManagerId == request.Id, cancellationToken))
            throw new ConflictException("The employee has direct reports. Reassign them before deleting this employee.");

        employee.IsDeleted = true;
        employee.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }
}
