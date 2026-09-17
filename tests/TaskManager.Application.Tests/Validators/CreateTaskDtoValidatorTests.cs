using FluentAssertions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using Xunit;

namespace TaskManager.Application.Tests.Validators;

public class CreateTaskDtoValidatorTests
{
    private readonly CreateTaskDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        var dto = new CreateTaskDto("", "Description", DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenTitleExceedsMaxLength()
    {
        var dto = new CreateTaskDto(new string('a', 201), "Description", DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDueDateIsInThePast()
    {
        var dto = new CreateTaskDto("Title", "Description", DateTime.UtcNow.AddDays(-1));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DueDate");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDtoIsValid()
    {
        var dto = new CreateTaskDto("Title", "Description", DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
