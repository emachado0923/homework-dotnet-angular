using FluentAssertions;
using NSubstitute;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        var dto = new RegisterDto("user@example.com", "Password123!", "Full Name");
        _userRepository.ExistsByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.RegisterAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Conflict);
    }

    [Fact]
    public async Task RegisterAsync_ShouldHashPasswordAndPersistUser_WhenEmailIsAvailable()
    {
        var dto = new RegisterDto("user@example.com", "Password123!", "Full Name");
        _userRepository.ExistsByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash(dto.Password).Returns("hashed-password");
        _userRepository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(true);
        _jwtTokenGenerator.GenerateToken(Arg.Any<User>())
            .Returns(new JwtToken("token-value", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.RegisterAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token-value");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == dto.Email && u.PasswordHash == "hashed-password"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        var dto = new LoginDto("user@example.com", "Password123!");
        _userRepository.GetByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _sut.LoginAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUnauthorized_WhenPasswordDoesNotMatch()
    {
        var dto = new LoginDto("user@example.com", "WrongPassword!");
        var user = new User { Email = dto.Email, PasswordHash = "hashed-password" };
        _userRepository.GetByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(dto.Password, user.PasswordHash).Returns(false);

        var result = await _sut.LoginAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ResultError.Unauthorized);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var dto = new LoginDto("user@example.com", "Password123!");
        var user = new User { Email = dto.Email, PasswordHash = "hashed-password", FullName = "Full Name" };
        _userRepository.GetByEmailAsync(dto.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(dto.Password, user.PasswordHash).Returns(true);
        _jwtTokenGenerator.GenerateToken(user)
            .Returns(new JwtToken("token-value", DateTime.UtcNow.AddHours(1)));

        var result = await _sut.LoginAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("token-value");
        result.Value.Email.Should().Be(dto.Email);
    }
}
