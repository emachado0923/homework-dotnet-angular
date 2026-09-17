using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskManager.Api.Extensions;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public class TasksController(
    ITaskService taskService,
    IValidator<CreateTaskDto> createValidator,
    IValidator<UpdateTaskDto> updateValidator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await taskService.GetAllForUserAsync(User.GetUserId(), cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await taskService.GetByIdAsync(User.GetUserId(), id, cancellationToken);
        return ToActionResult(result, result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(BuildModelState(validation));
        }

        var result = await taskService.CreateAsync(User.GetUserId(), dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(Guid id, UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(BuildModelState(validation));
        }

        var result = await taskService.UpdateAsync(User.GetUserId(), id, dto, cancellationToken);
        return ToActionResult(result, result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await taskService.DeleteAsync(User.GetUserId(), id, cancellationToken);
        if (!result.IsSuccess)
        {
            return result.Error == ResultError.NotFound
                ? NotFound(new { message = result.ErrorMessage })
                : Forbid();
        }

        return NoContent();
    }

    private IActionResult ToActionResult<T>(Result<T> result, T? value)
    {
        if (result.IsSuccess)
        {
            return Ok(value);
        }

        return result.Error switch
        {
            ResultError.NotFound => NotFound(new { message = result.ErrorMessage }),
            ResultError.Unauthorized => Forbid(),
            _ => BadRequest(new { message = result.ErrorMessage })
        };
    }

    private static ModelStateDictionary BuildModelState(FluentValidation.Results.ValidationResult validation)
    {
        var modelState = new ModelStateDictionary();
        foreach (var error in validation.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
        return modelState;
    }
}
