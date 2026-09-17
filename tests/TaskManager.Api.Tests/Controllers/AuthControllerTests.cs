using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManager.Application.DTOs;
using Xunit;

namespace TaskManager.Api.Tests.Controllers;

public class AuthControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ShouldReturn201AndToken_WhenRequestIsValid()
    {
        var dto = new RegisterDto($"{Guid.NewGuid()}@example.com", "Password123!", "New User");

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenPasswordIsWeak()
    {
        var dto = new RegisterDto($"{Guid.NewGuid()}@example.com", "weak", "New User");

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturn409_WhenEmailAlreadyRegistered()
    {
        var dto = new RegisterDto($"{Guid.NewGuid()}@example.com", "Password123!", "New User");
        await _client.PostAsJsonAsync("/api/auth/register", dto);

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ShouldReturn200AndToken_WhenCredentialsAreValid()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto(email, "Password123!", "New User"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, "Password123!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenPasswordIsWrong()
    {
        var email = $"{Guid.NewGuid()}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterDto(email, "Password123!", "New User"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, "WrongPassword!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
