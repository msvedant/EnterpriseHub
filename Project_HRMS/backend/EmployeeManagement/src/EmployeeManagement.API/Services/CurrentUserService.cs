using System.Security.Claims;
using EmployeeManagement.Application.Common.Abstractions;

namespace EmployeeManagement.API.Services;

public sealed class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = Principal?.FindFirst("sub")?.Value ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => Principal?.Identity?.IsAuthenticated == true ? Principal.FindFirst("email")?.Value : null;
}
