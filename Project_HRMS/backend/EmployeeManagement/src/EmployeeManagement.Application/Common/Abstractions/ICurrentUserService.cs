namespace EmployeeManagement.Application.Common.Abstractions;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
}
