using FluentAssertions;
using TaskManager.Infrastructure.Security;
using Xunit;

namespace TaskManager.Infrastructure.Tests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _sut = new();

    [Fact]
    public void Hash_ShouldReturnDifferentValue_ThanOriginalPassword()
    {
        var hash = _sut.Hash("Password123!");

        hash.Should().NotBe("Password123!");
    }

    [Fact]
    public void Verify_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var hash = _sut.Hash("Password123!");

        _sut.Verify("Password123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var hash = _sut.Hash("Password123!");

        _sut.Verify("WrongPassword!", hash).Should().BeFalse();
    }
}
