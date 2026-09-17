using FluentAssertions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.Application.Tests.Validators;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_ShouldFail_WhenEmailIsInvalid(string email)
    {
        var dto = new RegisterDto(email, "Password123!", "Full Name");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase123!")]
    [InlineData("NOLOWERCASE123!")]
    [InlineData("NoDigitsHere!")]
    public void Validate_ShouldFail_WhenPasswordDoesNotMeetComplexity(string password)
    {
        var dto = new RegisterDto("user@example.com", password, "Full Name");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_ShouldFail_WhenFullNameIsEmpty()
    {
        var dto = new RegisterDto("user@example.com", "Password123!", "");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDtoIsValid()
    {
        var dto = new RegisterDto("user@example.com", "Password123!", "Full Name");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}

public class LoginDtoValidatorTests
{
    private readonly LoginDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenEmailIsEmpty()
    {
        var dto = new LoginDto("", "Password123!");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_ShouldFail_WhenPasswordIsEmpty()
    {
        var dto = new LoginDto("user@example.com", "");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDtoIsValid()
    {
        var dto = new LoginDto("user@example.com", "Password123!");

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
