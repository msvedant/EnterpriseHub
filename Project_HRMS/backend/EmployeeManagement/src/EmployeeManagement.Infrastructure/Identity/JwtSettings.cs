using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Infrastructure.Identity;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>HMAC signing key. At least 32 characters; supply via environment variable / Key Vault, never commit it.</summary>
    [Required, MinLength(32)]
    public string Key { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
