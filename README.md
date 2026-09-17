# Task Management System

A full-stack Task Management System with JWT authentication and full CRUD, built with
**.NET 10 (Clean Architecture)** on the backend and **Angular 20 (standalone, signals)** on
the frontend. See [`GENAI_DOCS.md`](./GENAI_DOCS.md) for the AI-assisted development log.

## Prerequisites

Install these before running the project:

| Tool | Version | Check with |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0.x | `dotnet --version` |
| [Node.js](https://nodejs.org/) | 20.x or newer | `node --version` |
| npm | 10.x or newer (bundled with Node) | `npm --version` |

No separate database server is required — the API uses SQLite, and the database file is
created automatically on first run.

## 1. Clone the repository

```bash
git clone https://github.com/emachado0923/homework-dotnet-angular.git
cd homework-dotnet-angular
```

## 2. Run the backend API

```bash
dotnet restore
dotnet run --project src/Presentation/TaskManager.Api/TaskManager.Api.csproj
```

The API starts at **http://localhost:5122** (see
`src/Presentation/TaskManager.Api/Properties/launchSettings.json` for the exact ports).
On first run it creates `taskmanager.db` (SQLite) and seeds a demo user and two tasks.

- Swagger UI: http://localhost:5122/swagger

**Demo credentials:**

- Email: `admin@taskmanager.com`
- Password: `Admin123!`

## 3. Run the frontend (Angular)

In a second terminal:

```bash
cd src/Presentation/task-manager-ui
npm install
npm start
```

This runs `ng serve` and opens the app at **http://localhost:4200**. The frontend is
pre-configured (`src/environments/environment*.ts`) to call the API at
`http://localhost:5122/api`, and the API's CORS policy already allows
`http://localhost:4200` — no extra configuration is needed.

Log in with the demo credentials above, or register a new account.

## 4. Run the tests

Backend (xUnit, all 4 test projects — Domain, Application, Infrastructure, API integration):

```bash
dotnet test
```

Frontend (Karma/Jasmine — requires a local Chrome/Chromium installation):

```bash
cd src/Presentation/task-manager-ui
npm test
```

## Project structure

```
├── src/
│   ├── Core/
│   │   ├── TaskManager.Domain/         # Entities (User, TaskItem) and domain rules
│   │   └── TaskManager.Application/    # DTOs, validators, services, interfaces
│   ├── Infrastructure/
│   │   └── TaskManager.Infrastructure/ # EF Core, repositories, JWT, password hashing
│   └── Presentation/
│       ├── TaskManager.Api/            # ASP.NET Core Web API (controllers, auth, Swagger)
│       └── task-manager-ui/            # Angular 20 SPA
└── tests/                              # One xUnit test project per backend layer
```

## Troubleshooting

- **Port already in use**: change the port with `dotnet run --urls http://localhost:5122`
  (backend) or `ng serve --port 4300` (frontend, then update `apiUrl` if you also move the
  backend).
- **Frontend can't reach the API**: confirm the backend is running first and that
  `src/environments/environment.development.ts` still points at `http://localhost:5122/api`.
- **Reset the demo data**: stop the API and delete
  `src/Presentation/TaskManager.Api/taskmanager.db`, then run the API again to re-seed it.
