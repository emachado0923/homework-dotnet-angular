using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Infrastructure.Tests.Repositories;

public class TaskRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task AddAsync_Then_SaveChangesAsync_ShouldPersistTask()
    {
        await using var context = CreateContext();
        var sut = new TaskRepository(context);
        var task = new TaskItem { Title = "Test", UserId = Guid.NewGuid() };

        await sut.AddAsync(task);
        await sut.SaveChangesAsync();

        (await context.Tasks.FindAsync(task.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllByUserIdAsync_ShouldReturnOnlyTasksForThatUser()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        context.Tasks.AddRange(
            new TaskItem { Title = "Mine", UserId = userId },
            new TaskItem { Title = "Not mine", UserId = Guid.NewGuid() });
        await context.SaveChangesAsync();
        var sut = new TaskRepository(context);

        var result = await sut.GetAllByUserIdAsync(userId);

        result.Should().ContainSingle(t => t.Title == "Mine");
    }

    [Fact]
    public async Task Delete_Then_SaveChangesAsync_ShouldRemoveTask()
    {
        await using var context = CreateContext();
        var task = new TaskItem { Title = "To delete", UserId = Guid.NewGuid() };
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        var sut = new TaskRepository(context);

        sut.Delete(task);
        await sut.SaveChangesAsync();

        (await context.Tasks.FindAsync(task.Id)).Should().BeNull();
    }
}
