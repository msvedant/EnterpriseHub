using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Exceptions;
using EmployeeManagement.Application.Features.Auth;
using EmployeeManagement.Application.Features.Auth.Commands;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.UnitTests.Helpers;
using Moq;

namespace EmployeeManagement.UnitTests.Unit;

public class LoginCommandHandlerTests
{
    private readonly Mock<IPasswordService> _passwords = new();
    private readonly Mock<IJwtTokenService> _tokens = new();

    private LoginCommandHandler CreateHandler(Infrastructure.Persistence.AppDbContext db) =>
        new(db, _passwords.Object, _tokens.Object, TimeProvider.System);

    private static async Task<User> AddUserAsync(Infrastructure.Persistence.AppDbContext db)
    {
        var user = new User { Email = "admin@example.com", FullName = "Admin", PasswordHash = "hash", Role = UserRole.Admin };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Valid_credentials_return_a_token()
    {
        await using var db = TestDb.Create();
        await AddUserAsync(db);
        _passwords.Setup(p => p.Verify("hash", "secret")).Returns(true);
        _tokens.Setup(t => t.CreateToken(It.IsAny<User>())).Returns(new TokenResult("jwt-token", DateTime.UtcNow.AddHours(1)));

        var result = await CreateHandler(db).Handle(
            new LoginCommand(new LoginRequest { Email = "Admin@Example.com ", Password = "secret" }), CancellationToken.None);

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal("Admin", result.User.Role);
    }

    [Fact]
    public async Task Wrong_password_is_rejected_and_counted()
    {
        await using var db = TestDb.Create();
        var user = await AddUserAsync(db);
        _passwords.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedException>(() => CreateHandler(db).Handle(
            new LoginCommand(new LoginRequest { Email = "admin@example.com", Password = "bad" }), CancellationToken.None));

        Assert.Equal(1, user.FailedLoginAttempts);
        _tokens.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Account_locks_after_five_failures()
    {
        await using var db = TestDb.Create();
        var user = await AddUserAsync(db);
        _passwords.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var handler = CreateHandler(db);
        var command = new LoginCommand(new LoginRequest { Email = "admin@example.com", Password = "bad" });

        for (var i = 0; i < 5; i++)
            await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));

        Assert.NotNull(user.LockoutEnd);

        // Even the right password is refused while locked.
        _passwords.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Unknown_user_is_rejected()
    {
        await using var db = TestDb.Create();

        await Assert.ThrowsAsync<UnauthorizedException>(() => CreateHandler(db).Handle(
            new LoginCommand(new LoginRequest { Email = "nobody@example.com", Password = "x" }), CancellationToken.None));
    }
}
