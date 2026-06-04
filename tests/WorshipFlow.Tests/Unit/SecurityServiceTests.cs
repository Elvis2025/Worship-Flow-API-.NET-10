using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using WorshipFlow.Domain.Entities;
using WorshipFlow.Domain.Enums;
using WorshipFlow.Infrastructure.Security;
using Xunit;

namespace WorshipFlow.Tests.Unit;

public sealed class SecurityServiceTests
{
    [Fact]
    public void BCryptPasswordHasher_HashesAndVerifiesPassword()
    {
        var hasher = new BCryptPasswordHasher();

        var hash = hasher.Hash("Password123!");

        hash.Should().NotBe("Password123!");
        hasher.Verify("Password123!", hash).Should().BeTrue();
        hasher.Verify("wrong", hash).Should().BeFalse();
    }

    [Fact]
    public void JwtTokenService_CreatesAccessTokenRefreshTokenAndTenantClaims()
    {
        var tenantId = Guid.NewGuid();
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FullName = "Unit Tester",
            FirstName = "Unit",
            LastName = "Tester",
            Email = "unit@example.com",
            MainInstrument = "Piano",
            Status = UserStatus.Active
        };
        var service = new JwtTokenService(CreateConfiguration());

        var pair = service.CreateTokenPair(user, "device-1", "Unit Device", "127.0.0.1", "UnitTest");

        pair.AccessToken.Should().NotBeNullOrWhiteSpace();
        pair.RefreshToken.Should().NotBeNullOrWhiteSpace();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(pair.AccessToken);
        jwt.Claims.Should().Contain(x => x.Type == "tenant_id" && x.Value == tenantId.ToString());
        jwt.Claims.Should().Contain(x => x.Type == "device_id" && x.Value == "device-1");
    }

    [Fact]
    public void JwtTokenService_HashesRefreshTokensDeterministically()
    {
        var service = new JwtTokenService(CreateConfiguration());

        var first = service.HashToken("refresh-token");
        var second = service.HashToken("refresh-token");

        first.Should().Be(second);
        first.Should().NotBe("refresh-token");
    }

    private static IConfiguration CreateConfiguration() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "unit-test-secret-key-with-at-least-32-characters",
            ["Jwt:Issuer"] = "WorshipFlow.Tests",
            ["Jwt:Audience"] = "WorshipFlow.Tests",
            ["Jwt:AccessMinutes"] = "15"
        })
        .Build();
}
