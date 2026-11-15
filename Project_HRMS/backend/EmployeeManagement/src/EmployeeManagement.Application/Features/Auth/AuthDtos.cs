using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Features.Auth;

public sealed record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed record RegisterUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public UserRole Role { get; init; } = UserRole.Employee;
}

public sealed record UserDto(int Id, string Email, string FullName, string Role, bool IsActive);

public sealed record AuthResponse(string AccessToken, string TokenType, DateTime ExpiresAtUtc, UserDto User);
