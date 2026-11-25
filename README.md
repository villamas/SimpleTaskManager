# Simple Task Manager

A local To-Do List application built with .NET 11 Minimal API, EF Core, SQLite, and plain HTML, CSS, and JavaScript.

## Run locally

1. Install the .NET 11 SDK.
2. From the repository root, restore packages once:

   ```powershell
   dotnet restore SimpleTaskManager.slnx
   ```

3. Start the application:

   ```powershell
   dotnet run --project src/SimpleTaskManager.Api
   ```

4. Open [http://localhost:5000](http://localhost:5000).

The application binds to localhost and creates `src/SimpleTaskManager.Api/App_Data/tasks.db` on first startup. Tasks persist across normal restarts. After packages have been restored, the application runs locally without internet access or external services.

## API

- `GET /api/tasks?status=All|Pending|Completed`
- `GET /api/tasks/{id}`
- `POST /api/tasks` with `{"title":"Task title"}`
- `PATCH /api/tasks/{id}` with `{"title":"Updated title"}`
- `PUT /api/tasks/{id}/status` with `{"status":"Completed"}`
- `DELETE /api/tasks/{id}`
