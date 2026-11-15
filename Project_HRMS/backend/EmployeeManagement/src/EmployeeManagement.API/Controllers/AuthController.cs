using System.Security.Claims;
using EmployeeManagement.API.Models;
using EmployeeManagement.Application.Features.Auth;
using EmployeeManagement.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmployeeManagement.API.Controllers;

[Route("api/auth")]
public sealed class AuthController(ISender sender) : ApiControllerBase
{
    /// <summary>Exchanges credentials for a JWT access token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct) =>
        Success(await sender.Send(new LoginCommand(request), ct), "Login successful.");

    /// <summary>Creates a login account (Admin only).</summary>
    [HttpPost("register")]
    [Authorize(Policy = Policies.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken ct)
    {
        var user = await sender.Send(new RegisterUserCommand(request), ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserDto>.Ok(user, "User created successfully."));
    }

    /// <summary>Returns the identity carried by the current token.</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Success(new
    {
        Id = User.FindFirstValue("sub"),
        Email = User.FindFirstValue("email"),
        Name = User.FindFirstValue("name"),
        Role = User.FindFirstValue("role")
    });
}
