using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Api.Tests.Controllers;

public class TasksControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory = factory;

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid()}@example.com";
        var registerResponse = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterDto(email, "Password123!", "Task User"));
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Then_GetAll_ShouldReturnTheCreatedTask()
    {
        var client = await CreateAuthenticatedClientAsync();
        var dto = new CreateTaskDto("Write tests", "Cover TaskController", DateTime.UtcNow.AddDays(1));

        var createResponse = await client.PostAsJsonAsync("/api/tasks", dto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var getAllResponse = await client.GetAsync("/api/tasks");
        var tasks = await getAllResponse.Content.ReadFromJsonAsync<List<TaskDto>>();

        tasks.Should().ContainSingle(t => t.Title == "Write tests");
    }

    [Fact]
    public async Task Update_ShouldReturn403_WhenTaskBelongsToAnotherUser()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var createResponse = await ownerClient.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskDto("Owner task", "Description", DateTime.UtcNow.AddDays(1)));
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var otherClient = await CreateAuthenticatedClientAsync();
        var updateDto = new UpdateTaskDto("Hacked", "Description", TaskItemStatus.Completed, DateTime.UtcNow.AddDays(1));

        var response = await otherClient.PutAsJsonAsync($"/api/tasks/{created!.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_WhenUserOwnsTask()
    {
        var client = await CreateAuthenticatedClientAsync();
        var createResponse = await client.PostAsJsonAsync(
            "/api/tasks",
            new CreateTaskDto("To delete", "Description", DateTime.UtcNow.AddDays(1)));
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>();

        var response = await client.DeleteAsync($"/api/tasks/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenTitleIsEmpty()
    {
        var client = await CreateAuthenticatedClientAsync();
        var dto = new CreateTaskDto("", "Description", DateTime.UtcNow.AddDays(1));

        var response = await client.PostAsJsonAsync("/api/tasks", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
