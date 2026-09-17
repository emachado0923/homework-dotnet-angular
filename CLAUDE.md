# CLAUDE.md - Technical Assessment Execution Guide

## Project Context & Tech Stack
- **Project Goal**: Task Management System with Auth & Full CRUD using Clean Architecture + TDD + GenAI integration.
- **Backend**: .NET 10 Web API (C# 14 / .NET 10 SDK)
- **Frontend**: Angular 20 (Standalone Components, Signals, Control Flow, Modern RxJS/HttpClient)
- **Database**: EF Core 10 with SQLite (Development/Demo) or PostgreSQL
- **Testing**: xUnit, Moq / NSubstitute, FluentAssertions, WebApplicationFactory
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Auth**: JWT Bearer Authentication with ASP.NET Core Identity or Custom Password Hashing

---

## Architecture Overview (.NET 10 Clean Architecture)

```
TaskManagementSystem/
├── src/
│   ├── Core/
│   │   ├── TaskManager.Domain/         # Enterprise Logic & Entities (Zero Dependencies)
│   │   └── TaskManager.Application/    # Use Cases, DTOs, Interfaces, Business Validation
│   ├── Infrastructure/
│   │   └── TaskManager.Infrastructure/ # EF Core, DB Context, Repositories, JWT, Hasher
│   └── Presentation/
│       ├── TaskManager.Api/            # ASP.NET Core 10 Web API (Controllers, Swagger, Middleware)
│       └── task-manager-ui/            # Angular 20 SPA
└── tests/
    ├── TaskManager.Domain.Tests/       # Domain Unit Tests
    ├── TaskManager.Application.Tests/  # Business Logic & Validator Unit Tests
    ├── TaskManager.Infrastructure.Tests/ # Data Access Tests
    └── TaskManager.Api.Tests/          # Integration Tests (WebApplicationFactory)
```

---

## STEP 1: Solution Setup & CLI Commands

Run these commands to generate the exact solution layout:

```bash
# 1. Create Solution
dotnet new sln -n TaskManagerSystem

# 2. Create Core Layer Projects
dotnet new classlib -o src/Core/TaskManager.Domain -f net10.0
dotnet new classlib -o src/Core/TaskManager.Application -f net10.0

# 3. Create Infrastructure Layer Project
dotnet new classlib -o src/Infrastructure/TaskManager.Infrastructure -f net10.0

# 4. Create API Project
dotnet new webapi -o src/Presentation/TaskManager.Api -f net10.0

# 5. Create Test Projects
dotnet new xunit -o tests/TaskManager.Domain.Tests -f net10.0
dotnet new xunit -o tests/TaskManager.Application.Tests -f net10.0
dotnet new xunit -o tests/TaskManager.Infrastructure.Tests -f net10.0
dotnet new xunit -o tests/TaskManager.Api.Tests -f net10.0

# 6. Add Projects to Solution
dotnet sln add src/Core/TaskManager.Domain/TaskManager.Domain.csproj
dotnet sln add src/Core/TaskManager.Application/TaskManager.Application.csproj
dotnet sln add src/Infrastructure/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj
dotnet sln add src/Presentation/TaskManager.Api/TaskManager.Api.csproj
dotnet sln add tests/TaskManager.Domain.Tests/TaskManager.Domain.Tests.csproj
dotnet sln add tests/TaskManager.Application.Tests/TaskManager.Application.Tests.csproj
dotnet sln add tests/TaskManager.Infrastructure.Tests/TaskManager.Infrastructure.Tests.csproj
dotnet sln add tests/TaskManager.Api.Tests/TaskManager.Api.Tests.csproj

# 7. Configure Project References (Clean Architecture Rules)
dotnet add src/Core/TaskManager.Application/TaskManager.Application.csproj reference src/Core/TaskManager.Domain/TaskManager.Domain.csproj
dotnet add src/Infrastructure/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj reference src/Core/TaskManager.Application/TaskManager.Application.csproj
dotnet add src/Presentation/TaskManager.Api/TaskManager.Api.csproj reference src/Infrastructure/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj

# Test References
dotnet add tests/TaskManager.Domain.Tests/TaskManager.Domain.Tests.csproj reference src/Core/TaskManager.Domain/TaskManager.Domain.csproj
dotnet add tests/TaskManager.Application.Tests/TaskManager.Application.Tests.csproj reference src/Core/TaskManager.Application/TaskManager.Application.csproj
dotnet add tests/TaskManager.Infrastructure.Tests/TaskManager.Infrastructure.Tests.csproj reference src/Infrastructure/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj
dotnet add tests/TaskManager.Api.Tests/TaskManager.Api.Tests.csproj reference src/Presentation/TaskManager.Api/TaskManager.Api.csproj
```

---

## STEP 2: TDD & Domain Layer Implementation

### User Story (Business Specification)
> **As a** registered user  
> **I want to** securely log in and manage my tasks (Create, Read, Update, Delete with title, description, status, due date)  
> **So that** I can keep track of my daily productivity and ensure deadlines are met.

### TDD Cycle Rule
1. **RED**: Write a failing unit test first in `tests/TaskManager.Application.Tests/` or `tests/TaskManager.Domain.Tests/`.
2. **GREEN**: Write minimal C# code in `TaskManager.Domain` or `TaskManager.Application` to make the test pass.
3. **REFACTOR**: Clean up code, enforce C# 14 features, and ensure high quality.

### Core Domain Entities
Create in `TaskManager.Domain/Entities/`:

```csharp
namespace TaskManager.Domain.Entities;

public enum TaskItemStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public DateTime DueDate { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

---

## STEP 3: Application & Validation Layer

In `TaskManager.Application`:
1. Define DTOs (`TaskDto`, `CreateTaskDto`, `UpdateTaskDto`, `LoginDto`, `RegisterDto`, `AuthResponseDto`).
2. Define Interfaces (`ITaskRepository`, `IUserRepository`, `IJwtTokenGenerator`).
3. Add **FluentValidation** for incoming requests (e.g., Title is required, DueDate cannot be in the past).

### Unit Test Pattern Example (TDD)
```csharp
// tests/TaskManager.Application.Tests/Tasks/CreateTaskCommandTests.cs
using FluentAssertions;
using Xunit;

public class CreateTaskCommandTests
{
    [Fact]
    public void Validate_ShouldFail_WhenTitleIsEmpty()
    {
        // Arrange
        var validator = new CreateTaskDtoValidator();
        var dto = new CreateTaskDto("", "Description", DateTime.UtcNow.AddDays(1));

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }
}
```

---

## STEP 4: Infrastructure & Data Access (.NET 10)

In `TaskManager.Infrastructure`:
1. Implement `AppDbContext : DbContext`.
2. Add EF Core entity configurations using `IEntityTypeConfiguration<T>`.
3. Implement Repositories (`TaskRepository`, `UserRepository`).
4. Implement Seed Data (`DbInitializer`) providing default user credentials and initial tasks:
   - **Demo Email**: `admin@taskmanager.com`
   - **Demo Password**: `Admin123!`

```csharp
// Seed Data Execution
public static class DbInitializer
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Users.Any())
        {
            var demoUser = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Email = "admin@taskmanager.com",
                FullName = "Demo User",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!")
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
}
```

---

## STEP 5: API Layer & JWT Auth

In `TaskManager.Api`:
1. Configure CORS to allow Angular (`http://localhost:4200`).
2. Add JWT Authentication middleware (`builder.Services.AddAuthentication()`).
3. Create Endpoints:
   - `POST /api/auth/register` (Public)
   - `POST /api/auth/login` (Public)
   - `GET /api/tasks` (Authorized)
   - `POST /api/tasks` (Authorized)
   - `PUT /api/tasks/{id}` (Authorized)
   - `DELETE /api/tasks/{id}` (Authorized)

---

## STEP 6: Frontend - Angular 20 Implementation

Initialize Angular application inside `src/Presentation/`:

```bash
cd src/Presentation
npx @angular/cli@20 new task-manager-ui --standalone --routing --style=scss --skip-git
cd task-manager-ui
```

### Architecture Features to Use
- **Standalone Components** (`standalone: true`).
- **Signals** for reactive state (`signal()`, `computed()`).
- **Control Flow** (`@if`, `@for`, `@switch`).
- **Functional Route Guards** for auth protection (`canActivate: [authGuard]`).
- **HttpInterceptorFn** for injecting JWT Authorization Bearer headers.

### Core Angular Components
1. `LoginComponent` / `RegisterComponent`: Auth forms with validation.
2. `TaskListComponent`: Displays tasks with filter tabs (All, Pending, Completed).
3. `TaskFormComponent`: Modal or page to Create/Edit tasks with datepicker.
4. `NavbarComponent`: User status, email display, logout button.

---

## STEP 7: GenAI Section Documentation

As required by the assessment specification, document your AI interaction details in `GENAI_DOCS.md`:

### 1. Initial Prompt Provided
```text
"Act as a Principal .NET Architect. Create an ASP.NET Core Web API controller for Task CRUD operations following Clean Architecture. Use DTOs, response wrapping with API Results, async/await, and proper HTTP response codes (200, 201, 204, 400, 404, 401). Ensure endpoints check user ownership before mutating tasks."
```

### 2. Validation & Refinement Log
- **AI Suggestion**: The initial AI scaffold used primitive types directly in the controller and omitted user ownership verification on `DELETE /api/tasks/{id}`.
- **Correction Executed**: Added domain rule validation `task.UserId == currentUserId` in Application Service layer to prevent unauthorized cross-user data modification.
- **Edge Cases Handled**:
  - Expiry of JWT token handled gracefully in Angular HttpInterceptor.
  - Expired `DueDate` highlighted visually in red on Angular UI.

---

## STEP 8: Presentation & Live Code Review Script

Prepare to present via Google Meet / Zoom with screen sharing:

1. **User Story & Overview (2 mins)**: Explain the business problem, user story, and domain rules.
2. **Architecture Breakdown (3 mins)**: Show Clean Architecture separation (`Domain` -> `Application` -> `Infrastructure` -> `API`).
3. **Live App Demo (3 mins)**:
   - Login using seed credentials (`admin@taskmanager.com` / `Admin123!`).
   - Create a task, edit status, filter list, delete item.
   - Show responsive UI behavior.
4. **TDD & Unit Testing (2 mins)**: Run `dotnet test` in terminal showing 100% passing tests across Domain and Application layers.
5. **GenAI Strategy (2 mins)**: Explain prompt engineering workflow, code validation, and edge-case hardening.

---

## Verification Commands

```bash
# Run all unit and integration tests
dotnet test

# Run .NET API
dotnet run --project src/Presentation/TaskManager.Api/TaskManager.Api.csproj

# Run Angular App
cd src/Presentation/task-manager-ui && ng serve
```
