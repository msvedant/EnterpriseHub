using EmployeeManagement.Application.Common.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace EmployeeManagement.Infrastructure.Identity;

/// <summary>PBKDF2 password hashing (ASP.NET Core Identity v3 format, per-password salt).</summary>
public sealed class PasswordService : IPasswordService
{
    private static readonly object Subject = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(Subject, password);

    public bool Verify(string passwordHash, string password)
    {
        if (string.IsNullOrEmpty(passwordHash))
            return false;

        var result = _hasher.VerifyHashedPassword(Subject, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
