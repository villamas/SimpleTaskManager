# Simple Task Manager — Technical Design

**Version:** 0.2 — Approved  
**Requirements baseline:** v0.2

## Architecture and stack

One ASP.NET Core Minimal API application serves static HTML/CSS/Vanilla JavaScript and a same-origin JSON API on localhost. Endpoint handlers use EF Core DbContext directly with local SQLite. Use .NET 11, EF Core SQLite, WebApplicationFactory, Microsoft.NET.Test.Sdk, xUnit, and its test runner; verify compatible package versions during setup. Node/npm are not runtime dependencies.

Do not use separate servers, frontend frameworks/builds, CORS, Clean Architecture, repositories, CQRS, MediatR, Docker, cloud services, or CI/CD.

## Project structure

```text
SimpleTaskManager/
  SimpleTaskManager.slnx
  src/SimpleTaskManager.Api/
    Program.cs  appsettings.json
    Models/TaskItem.cs  Data/AppDbContext.cs
    Contracts/TaskContracts.cs  Endpoints/TaskEndpoints.cs
    wwwroot/index.html  styles.css  app.js
    App_Data/tasks.db
  tests/SimpleTaskManager.Tests/TaskApiTests.cs
  Assessment/  .gitignore  README.md
```

Preserve `feature/createSimpleTask`, unrelated changes, and the Assessment exclusion. Ignore database journals, build artifacts, and temporary test files.

## SQLite schema

The single `Tasks` table contains `Id INTEGER PRIMARY KEY AUTOINCREMENT`, `Title TEXT NOT NULL` with length 1–255, and `Status TEXT NOT NULL` restricted to Pending or Completed with default Pending. Duplicate titles are allowed. No other attributes, relationships, or indexes are needed. Sort by ascending ID; gaps are acceptable. Store the database at `src/SimpleTaskManager.Api/App_Data/tasks.db`, relative to content root. Create/reuse it with `EnsureCreated`; no migrations are required for this fixed schema.

## API endpoints and contracts

| Method | Route | Success |
|---|---|---|
| GET | `/api/tasks?status=All\|Pending\|Completed` | 200, ascending-ID array |
| GET | `/api/tasks/{id}` | 200, task |
| POST | `/api/tasks` | 201, task and Location header |
| PATCH | `/api/tasks/{id}` | 200, updated task; title only |
| PUT | `/api/tasks/{id}/status` | 200, updated task |
| DELETE | `/api/tasks/{id}` | 204, no body |

Response: `{"id":1,"title":"Sample task A","status":"Pending"}`. POST/PATCH accept `{"title":"..."}`; POST assigns ID and Pending. Status accepts `{"status":"Completed"}`. Omitted status means All; empty results are `[]`. Use 400 for invalid input/status/filter/ID/malformed requests, 404 for nonexistent tasks, 415 for unsupported content type, and 500 for unexpected failures. Use understandable Problem Details where applicable.

## Validation and errors

Trim titles and reject null, empty, or over-255-character values through shared API validation. Accept only Pending or Completed. Frontend validation is convenience only. Verify existence before mutations and return 404 without affecting other records. Use parameterized EF queries, safe plain-text DOM rendering, built-in exception handling, local logging, and no stack traces in responses. Bind only to loopback; no authentication, authorization, or CORS.

## Frontend

Provide one page with title input/Add, All/Pending/Completed filters, task list, status controls, inline Edit with Save/Cancel, Delete, and simple error/empty-state feedback. Use native browser deletion confirmation, Fetch API with relative URLs, and in-memory current filter/list/editing state only. Refresh with the current filter after successful mutations and clear the creation input after success. No external fonts/CDNs, browser storage, advanced components, or build tools.

## Approved testing strategy

Use xUnit and WebApplicationFactory with real temporary SQLite files isolated per test; use synthetic data and clean them up. Limit automated testing to these eight integration scenarios:

| ID | Scenario |
|---|---|
| IT-001 | Create/retrieve a task; verify ID and Pending status. |
| IT-002 | Reject invalid creation/edit titles without changing data. |
| IT-003 | Update title while preserving ID and status. |
| IT-004 | Delete and verify absence. |
| IT-005 | Change Pending to Completed and back. |
| IT-006 | Filter by status; verify ordering and unchanged data. |
| IT-007 | Nonexistent-task operations fail gracefully. |
| IT-008 | Restart the test host with the same database; verify persisted creation, editing, status, and deletion. |

Do not add an extensive unit suite, mocking framework, dedicated test abstractions, or mandatory coverage target. Manually verify all acceptance criteria, including empty/default state, confirm/cancel deletion, trimming/boundaries, duplicates, completed editing, restart persistence, loopback/local resources, responsiveness, errors, synthetic data, and Edge/Chrome compatibility.

### Optional Playwright MCP

Only during browser validation, after the app is running, verify connectivity and run one synthetic workflow: open localhost, create a task, edit its title, mark it Completed, delete and accept confirmation, and verify absence. Record successful MCP connection/tool logs, screenshots, and expected-versus-actual pass/fail results. If unavailable, perform it manually and do not claim MCP usage. It does not replace integration tests or human review.

## Local commands

From `C:\code\SimpleTaskManager`:

```powershell
dotnet restore SimpleTaskManager.slnx
dotnet run --project src/SimpleTaskManager.Api
dotnet run --project src/SimpleTaskManager.Api --no-restore
dotnet test SimpleTaskManager.slnx --no-restore
```

Use `http://localhost:5000` when available; document another local port if needed. No npm build, database server, or migration command is required.

## Boundaries

Build incrementally: verify environment, implement schema, API, frontend, eight tests, then manual/browser validation. Do not commit, push, merge, or switch branches automatically. Report actual changes, results, issues, limitations, and deviations. Scope or design changes require explicit human approval.
