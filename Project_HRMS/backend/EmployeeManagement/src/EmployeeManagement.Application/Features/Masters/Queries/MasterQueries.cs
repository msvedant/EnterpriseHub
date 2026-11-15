using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Models;
using MediatR;

namespace EmployeeManagement.Application.Features.Masters.Queries;

public sealed record GetMasterByIdQuery(MasterKind Kind, int Id) : IRequest<MasterDto>;

public sealed record GetMastersQuery(MasterKind Kind, MasterListFilter Filter) : IRequest<PagedResult<MasterDto>>;

public sealed class GetMasterByIdQueryHandler(IAppDbContext db) : IRequestHandler<GetMasterByIdQuery, MasterDto>
{
    public Task<MasterDto> Handle(GetMasterByIdQuery request, CancellationToken cancellationToken) =>
        MasterRegistry.Get(request.Kind).GetAsync(db, request.Id, cancellationToken);
}

public sealed class GetMastersQueryHandler(IAppDbContext db) : IRequestHandler<GetMastersQuery, PagedResult<MasterDto>>
{
    public Task<PagedResult<MasterDto>> Handle(GetMastersQuery request, CancellationToken cancellationToken) =>
        MasterRegistry.Get(request.Kind).ListAsync(db, request.Filter, cancellationToken);
}
