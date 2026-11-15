using EmployeeManagement.Domain.Entities;

namespace EmployeeManagement.Application.Common.Abstractions;

public sealed record TokenResult(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    TokenResult CreateToken(User user);
}
