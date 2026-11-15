using EmployeeManagement.API.Models;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Features.Employees;
using EmployeeManagement.Application.Features.Employees.Commands;
using EmployeeManagement.Application.Features.Employees.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers;

[Route("api/employees")]
public sealed class EmployeesController(ISender sender) : ApiControllerBase
{
    /// <summary>Paged, sortable and filterable employee list (no sensitive fields).</summary>
    [HttpGet]
    [Authorize(Policy = Policies.EmployeesRead)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EmployeeListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetEmployeesQuery query, CancellationToken ct) =>
        Success(await sender.Send(query, ct));

    /// <summary>Employee with family, address, education and experience details.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = Policies.EmployeesRead)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        Success(await sender.Send(new GetEmployeeByIdQuery(id), ct));

    /// <summary>Creates the employee and all detail rows in a single transaction.</summary>
    [HttpPost]
    [Authorize(Policy = Policies.EmployeesWrite)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] EmployeeRequest request, CancellationToken ct)
    {
        var created = await sender.Send(new CreateEmployeeCommand(request), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<EmployeeDto>.Ok(created, "Employee created successfully."));
    }

    /// <summary>
    /// Updates the employee. Detail items with an id are updated, items without an id are added,
    /// and existing items missing from the request are removed. Requires the current rowVersion.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = Policies.EmployeesWrite)]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeRequest request, CancellationToken ct) =>
        Success(await sender.Send(new UpdateEmployeeCommand(id, request), ct), "Employee updated successfully.");

    /// <summary>Activates or deactivates the employee.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = Policies.EmployeesWrite)]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusRequest request, CancellationToken ct)
    {
        await sender.Send(new SetEmployeeStatusCommand(id, request.IsActive), ct);
        return Success(request.IsActive ? "Employee activated." : "Employee deactivated.");
    }

    /// <summary>Soft deletes the employee (Admin only).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await sender.Send(new DeleteEmployeeCommand(id), ct);
        return Success("Employee deleted.");
    }
}
