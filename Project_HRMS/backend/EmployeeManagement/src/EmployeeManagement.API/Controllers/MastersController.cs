using EmployeeManagement.API.Models;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Features.Masters;
using EmployeeManagement.Application.Features.Masters.Commands;
using EmployeeManagement.Application.Features.Masters.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;

/// <summary>
/// CRUD for all master tables. {master} is one of: departments, designations, employee-types,
/// relationships, countries, states, cities.
/// </summary>
[Route("api/masters/{master}")]
public sealed class MastersController(ISender sender) : ApiControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<MasterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(string master, [FromQuery] MasterListFilter filter, CancellationToken ct) =>
        Success(await sender.Send(new GetMastersQuery(Resolve(master), filter), ct));

    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string master, int id, CancellationToken ct) =>
        Success(await sender.Send(new GetMasterByIdQuery(Resolve(master), id), ct));

    [HttpPost]
    [Authorize(Policy = Policies.MastersWrite)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(string master, [FromBody] MasterRequest request, CancellationToken ct)
    {
        var created = await sender.Send(new CreateMasterCommand(Resolve(master), request), ct);
        return CreatedAtAction(nameof(GetById), new { master, id = created.Id },
            ApiResponse<MasterDto>.Ok(created, "Created successfully."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.MastersWrite)]
    [ProducesResponseType(typeof(ApiResponse<MasterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string master, int id, [FromBody] MasterRequest request, CancellationToken ct) =>
        Success(await sender.Send(new UpdateMasterCommand(Resolve(master), id, request), ct), "Updated successfully.");

    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = Policies.MastersWrite)]
    public async Task<IActionResult> SetStatus(string master, int id, [FromBody] SetStatusRequest request, CancellationToken ct)
    {
        await sender.Send(new SetMasterStatusCommand(Resolve(master), id, request.IsActive), ct);
        return Success(request.IsActive ? "Activated successfully." : "Deactivated successfully.");
    }

    /// <summary>Soft delete. Rejected with 409 when the record is still referenced.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string master, int id, CancellationToken ct)
    {
        await sender.Send(new DeleteMasterCommand(Resolve(master), id), ct);
        return Success("Deleted successfully.");
    }

    private static MasterKind Resolve(string slug) =>
        MasterRegistry.TryParseSlug(slug, out var kind) ? kind : throw new NotFoundException("Master", slug);
}
