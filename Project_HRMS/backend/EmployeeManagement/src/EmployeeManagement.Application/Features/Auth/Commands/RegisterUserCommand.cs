using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Application.Features.Auth.Commands;

public sealed record RegisterUserCommand(RegisterUserRequest Data) : IRequest<UserDto>;

public sealed class RegisterUserCommandHandler(IAppDbContext db, IPasswordService passwords)
    : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var r = request.Data;
        var email = r.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
            throw new ConflictException("A user with this email address already exists.");

        var user = new User
        {
            Email = email,
            FullName = r.FullName.Trim(),
            Role = r.Role,
            PasswordHash = passwords.Hash(r.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return LoginCommandHandler.ToDto(user);
    }
}
