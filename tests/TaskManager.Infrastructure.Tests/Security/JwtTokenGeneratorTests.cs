using FluentAssertions;
using Microsoft.Extensions.Options;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Security;
using Xunit;

namespace TaskManager.Infrastructure.Tests.Security;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _sut;
    private readonly JwtSettings _settings = new()
    {
        SecretKey = "this-is-a-very-long-secret-key-used-only-for-unit-tests-12345",
        Issuer = "TaskManager.Api",
        Audience = "TaskManager.Client",
        ExpiryMinutes = 60
    };

    public JwtTokenGeneratorTests()
    {
        _sut = new JwtTokenGenerator(Options.Create(_settings));
    }

    [Fact]
    public void GenerateToken_ShouldReturnNonEmptyToken_WithFutureExpiry()
    {
        var user = new User { Email = "user@example.com", FullName = "Full Name" };

        var result = _sut.GenerateToken(user);

        result.Value.Should().NotBeNullOrWhiteSpace();
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void GenerateToken_ShouldEmbedUserIdAndEmailClaims()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", FullName = "Full Name" };

        var result = _sut.GenerateToken(user);
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Value);

        token.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        token.Claims.Should().Contain(c => c.Type == System.Security.Claims.ClaimTypes.Email && c.Value == user.Email);
    }
}
