using EmployeeManagement.Application.Common.Abstractions;
using MediatR;

namespace EmployeeManagement.Application.Features.Masters.Commands;

public sealed record CreateMasterCommand(MasterKind Kind, MasterRequest Data) : IRequest<MasterDto>;

public sealed record UpdateMasterCommand(MasterKind Kind, int Id, MasterRequest Data) : IRequest<MasterDto>;

public sealed record DeleteMasterCommand(MasterKind Kind, int Id) : IRequest;

public sealed record SetMasterStatusCommand(MasterKind Kind, int Id, bool IsActive) : IRequest;

public sealed class CreateMasterCommandHandler(IAppDbContext db) : IRequestHandler<CreateMasterCommand, MasterDto>
{
    public Task<MasterDto> Handle(CreateMasterCommand request, CancellationToken cancellationToken) =>
        MasterRegistry.Get(request.Kind).CreateAsync(db, request.Data, cancellationToken);
}

public sealed class UpdateMasterCommandHandler(IAppDbContext db) : IRequestHandler<UpdateMasterCommand, MasterDto>
{
    public Task<MasterDto> Handle(UpdateMasterCommand request, CancellationToken cancellationToken) =>
        MasterRegistry.Get(request.Kind).UpdateAsync(db, request.Id, request.Data, cancellationToken);
}

public sealed class DeleteMasterCommandHandler(IAppDbContext db) : IRequestHandler<DeleteMasterCommand>
{
    public Task Handle(DeleteMasterCommand request, CancellationToken cancellationToken) =>
        MasterRegistry.Get(request.Kind).DeleteAsync(db, request.Id, cancellationToken);
}

public sealed class SetMasterStatusCommandHandler(IAppDbContext db) : IRequestHandler<SetMasterStatusCommand>
{
    public Task Handle(SetMasterStatusCommand request, CancellationToken cancellationToken) =>
        MasterRegistry.Get(request.Kind).SetActiveAsync(db, request.Id, request.IsActive, cancellationToken);
}
