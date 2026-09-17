using FluentAssertions;
using TaskManager.Application.DTOs;
using TaskManager.Application.Validators;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Application.Tests.Validators;

public class UpdateTaskDtoValidatorTests
{
    private readonly UpdateTaskDtoValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        var dto = new UpdateTaskDto("", "Description", TaskItemStatus.Pending, DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_ShouldFail_WhenStatusIsNotDefined()
    {
        var dto = new UpdateTaskDto("Title", "Description", (TaskItemStatus)99, DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDtoIsValid()
    {
        var dto = new UpdateTaskDto("Title", "Description", TaskItemStatus.InProgress, DateTime.UtcNow.AddDays(1));

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
