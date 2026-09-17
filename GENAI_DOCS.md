# GenAI Usage Documentation

This project was built with Claude Code (Anthropic) acting as a pair-programming assistant, following a strict TDD (RED → GREEN → REFACTOR) workflow across the Clean Architecture layers described in `CLAUDE.md`.

## 1. Initial Prompt Provided

```text
"Act as a Principal .NET Architect. Create an ASP.NET Core Web API controller for Task CRUD
operations following Clean Architecture. Use DTOs, response wrapping with API Results,
async/await, and proper HTTP response codes (200, 201, 204, 400, 404, 401). Ensure endpoints
check user ownership before mutating tasks."
```

This prompt (embedded in the project's `CLAUDE.md`) drove the shape of `TasksController`,
the `Result<T>` wrapper in `TaskManager.Application.Common`, and the ownership checks inside
`TaskService`.

## 2. Validation & Refinement Log

- **AI Suggestion**: The first scaffold for `TasksController` mapped every failure to a
  generic `400 BadRequest`, including the case where a task belonged to another user.
- **Correction Executed**: Introduced a `ResultError` enum (`NotFound`, `Unauthorized`,
  `Validation`, `Conflict`) so `TaskService.UpdateAsync` / `DeleteAsync` can distinguish
  "task does not exist" (404) from "task exists but you don't own it" (403 Forbidden via
  `Forbid()`), matching the domain rule `task.BelongsTo(userId)` defined directly on the
  `TaskItem` entity and covered by `TaskItemTests.BelongsTo_*`.
- **AI Suggestion**: Swashbuckle/Microsoft.OpenApi security-scheme wiring initially used the
  legacy `OpenApiSecurityScheme.Reference` API.
- **Correction Executed**: The resolved `Microsoft.OpenApi` 2.x package moved that model to
  `Microsoft.OpenApi.OpenApiSecuritySchemeReference` and relocated all `OpenApi*` types out of
  the old `Microsoft.OpenApi.Models` namespace. Verified by inspecting the installed package
  assembly directly and adjusted `Program.cs` accordingly instead of guessing an API surface
  from (possibly stale) training data.
- **AI Suggestion**: Use FluentAssertions latest (`8.x`).
- **Correction Executed**: Pinned FluentAssertions to `7.0.0`, the last version released under
  the Apache-2.0 license, to avoid the commercial licensing terms introduced in `8.x` for an
  assessment deliverable that may be reviewed commercially.
- **Edge Cases Handled**:
  - Expiry of the JWT token is handled by the Angular `authInterceptor`: any `401` response
    triggers `AuthService.logout()` and a redirect to `/login`.
  - An expired `DueDate` is flagged by the domain method `TaskItem.IsOverdue()` (exposed as
    `isOverdue` on `TaskDto`) and highlighted in red on the Angular task list
    (`.task-card.overdue`, `.overdue-text`).
  - Weak passwords are rejected both client-side (Angular `Validators.pattern`) and
    server-side (`RegisterDtoValidator`), so the rule is enforced even if a request bypasses
    the SPA.
  - Duplicate registration emails return `409 Conflict` instead of a generic `400`, verified by
    `AuthControllerTests.Register_ShouldReturn409_WhenEmailAlreadyRegistered`.

## 3. TDD Workflow Followed

For every layer, tests were written first and observed failing (RED) before the minimal
implementation was added (GREEN), then refactored for clarity:

- `TaskManager.Domain.Tests` — entity invariants (`IsOverdue`, `MarkAsCompleted`, `BelongsTo`).
- `TaskManager.Application.Tests` — FluentValidation rules and `TaskService`/`AuthService`
  business logic, using NSubstitute to mock `ITaskRepository`, `IUserRepository`,
  `IPasswordHasher`, and `IJwtTokenGenerator`.
- `TaskManager.Infrastructure.Tests` — EF Core repositories against an in-memory provider,
  plus `BCryptPasswordHasher` and `JwtTokenGenerator` behavior.
- `TaskManager.Api.Tests` — full HTTP integration tests via `WebApplicationFactory<Program>`
  against an in-memory SQLite connection, covering register/login and the task CRUD ownership
  rules end to end.

Final result: **58/58 tests passing** across all four test projects (`dotnet test`).
