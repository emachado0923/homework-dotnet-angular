using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Seed;

public static class DbInitializer
{
    public static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string DemoEmail = "admin@taskmanager.com";
    public const string DemoPassword = "Admin123!";

    public static void Seed(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Users.Any())
        {
            return;
        }

        var demoUser = new User
        {
            Id = DemoUserId,
            Email = DemoEmail,
            FullName = "Demo User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword)
        };
        context.Users.Add(demoUser);

        context.Tasks.AddRange(
            new TaskItem
            {
                Title = "Complete Technical Assessment",
                Description = "Build .NET 10 and Angular 20 Clean Architecture App",
                Status = TaskItemStatus.InProgress,
                DueDate = DateTime.UtcNow.AddDays(2),
                UserId = demoUser.Id
            },
            new TaskItem
            {
                Title = "Prepare Presentation Slides",
                Description = "Summarize user story, architecture, and GenAI usage",
                Status = TaskItemStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(3),
                UserId = demoUser.Id
            }
        );

        context.SaveChanges();
    }
}
