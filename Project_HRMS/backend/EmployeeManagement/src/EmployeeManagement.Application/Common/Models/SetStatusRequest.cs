namespace EmployeeManagement.Application.Common.Models;

public sealed record SetStatusRequest
{
    public bool IsActive { get; init; }
}
