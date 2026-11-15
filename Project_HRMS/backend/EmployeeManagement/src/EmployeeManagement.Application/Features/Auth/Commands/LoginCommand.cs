using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Auth.Commands;

public sealed record LoginCommand(LoginRequest Data) : IRequest<AuthResponse>;

public sealed class LoginCommandHandler(
    IAppDbContext db,
    IPasswordService passwords,
    IJwtTokenService tokens,
    TimeProvider time) : IRequestHandler<LoginCommand, AuthResponse>
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const string InvalidCredentials = "Invalid email or password.";

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Data.Email.Trim().ToLowerInvariant();
        var now = time.GetUtcNow().UtcDateTime;

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        if (user is null || !user.IsActive)
            throw new UnauthorizedException(InvalidCredentials);

        if (user.LockoutEnd is { } lockoutEnd && lockoutEnd > now)
            throw new UnauthorizedException("The account is temporarily locked. Please try again later.");

        if (!passwords.Verify(user.PasswordHash, request.Data.Password))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                user.LockoutEnd = now.Add(LockoutDuration);
                user.FailedLoginAttempts = 0;
            }

            await db.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(InvalidCredentials);
        }

        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = now;
        await db.SaveChangesAsync(cancellationToken);

        var token = tokens.CreateToken(user);
        return new AuthResponse(token.AccessToken, "Bearer", token.ExpiresAtUtc, ToDto(user));
    }

    internal static UserDto ToDto(User u) => new(u.Id, u.Email, u.FullName, u.Role.ToString(), u.IsActive);
}
