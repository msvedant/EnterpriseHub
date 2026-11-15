using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace EmployeeManagement.UnitTests.Unit;

public class PasswordAndJwtTests
{
    [Fact]
    public void Password_hash_round_trips_and_is_salted()
    {
        var service = new PasswordService();
        var first = service.Hash("Str0ng!Passw0rd");
        var second = service.Hash("Str0ng!Passw0rd");

        Assert.NotEqual(first, second);
        Assert.True(service.Verify(first, "Str0ng!Passw0rd"));
        Assert.False(service.Verify(first, "wrong"));
    }

    [Fact]
    public void Jwt_contains_expected_claims()
    {
        var settings = new JwtSettings
        {
            Issuer = "issuer", Audience = "audience", ExpiryMinutes = 30,
            Key = "unit-test-signing-key-with-more-than-32-characters"
        };
        var service = new JwtTokenService(Options.Create(settings), TimeProvider.System);
        var user = new User { Id = 7, Email = "hr@example.com", FullName = "HR User", Role = UserRole.HR };

        var token = service.CreateToken(user);
        var jwt = new JsonWebTokenHandler().ReadJsonWebToken(token.AccessToken);

        Assert.Equal("7", jwt.GetClaim("sub").Value);
        Assert.Equal("HR", jwt.GetClaim("role").Value);
        Assert.Equal("issuer", jwt.Issuer);
        Assert.True(token.ExpiresAtUtc > DateTime.UtcNow);
    }
}
