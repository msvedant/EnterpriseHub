using System.Security.Claims;
using System.Text;
using EmployeeManagement.Application.Common.Abstractions;
using EmployeeManagement.Application.Common.Constants;
using EmployeeManagement.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtSettings> options, TimeProvider time) : IJwtTokenService
{
    private readonly JsonWebTokenHandler _handler = new();

    public TokenResult CreateToken(User user)
    {
        var settings = options.Value;
        var now = time.GetUtcNow().UtcDateTime;
        var expires = now.AddMinutes(settings.ExpiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimNames.Name, user.FullName),
                new Claim(ClaimNames.Role, user.Role.ToString())
            ]),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            NotBefore = now,
            IssuedAt = now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        return new TokenResult(_handler.CreateToken(descriptor), expires);
    }
}
