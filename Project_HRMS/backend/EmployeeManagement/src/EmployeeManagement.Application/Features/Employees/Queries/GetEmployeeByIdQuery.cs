using EmployeeManagement.Application.Common.Abstractions;
using MediatR;

namespace EmployeeManagement.Application.Features.Employees.Queries;

public sealed record GetEmployeeByIdQuery(int Id) : IRequest<EmployeeDto>;

public sealed class GetEmployeeByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto>
{
    public Task<EmployeeDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken) =>
        EmployeeReader.GetByIdAsync(db, request.Id, cancellationToken);
}
