using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Infrastructure.Tests.Repositories;

public class UserRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ExistsByEmailAsync_ShouldReturnTrue_WhenUserWithEmailExists()
    {
        await using var context = CreateContext();
        context.Users.Add(new User { Email = "user@example.com", FullName = "User" });
        await context.SaveChangesAsync();
        var sut = new UserRepository(context);

        (await sut.ExistsByEmailAsync("user@example.com")).Should().BeTrue();
    }

    [Fact]
    public async Task GetByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        await using var context = CreateContext();
        var sut = new UserRepository(context);

        (await sut.GetByEmailAsync("missing@example.com")).Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_Then_SaveChangesAsync_ShouldPersistUser()
    {
        await using var context = CreateContext();
        var sut = new UserRepository(context);
        var user = new User { Email = "new@example.com", FullName = "New User" };

        await sut.AddAsync(user);
        await sut.SaveChangesAsync();

        (await context.Users.FindAsync(user.Id)).Should().NotBeNull();
    }
}
