using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SimpleTaskManager.Api.Contracts;
using SimpleTaskManager.Api.Data;
using SimpleTaskManager.Api.Models;

namespace SimpleTaskManager.Api.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks");
        group.MapGet("", GetTasks);
        group.MapGet("/{id}", GetTask);
        group.MapPost("", CreateTask);
        group.MapPatch("/{id}", UpdateTask);
        group.MapPut("/{id}/status", UpdateStatus);
        group.MapDelete("/{id}", DeleteTask);
    }

    private static async Task<IResult> GetTasks(string? status, AppDbContext db, CancellationToken cancellationToken)
    {
        var filter = status ?? "All";
        if (filter is not ("All" or "Pending" or "Completed"))
            return BadRequest("Status filter must be All, Pending, or Completed.");

        var query = db.Tasks.AsNoTracking();
        if (filter != "All")
            query = query.Where(task => task.Status == filter);

        var tasks = await query.OrderBy(task => task.Id)
            .Select(task => new TaskResponse(task.Id, task.Title, task.Status))
            .ToListAsync(cancellationToken);
        return Results.Ok(tasks);
    }

    private static async Task<IResult> GetTask(string id, AppDbContext db, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var taskId)) return BadRequest("Task ID must be a positive integer.");
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        return task is null ? NotFound() : Results.Ok(ToResponse(task));
    }

    private static async Task<IResult> CreateTask(TitleRequest? request, AppDbContext db, CancellationToken cancellationToken)
    {
        if (!TryNormalizeTitle(request?.Title, out var title)) return BadRequest("Title must contain 1 to 255 characters.");

        var task = new TaskItem { Title = title };
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Created($"/api/tasks/{task.Id}", ToResponse(task));
    }

    private static async Task<IResult> UpdateTask(string id, TitleRequest? request, AppDbContext db, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var taskId)) return BadRequest("Task ID must be a positive integer.");
        if (!TryNormalizeTitle(request?.Title, out var title)) return BadRequest("Title must contain 1 to 255 characters.");

        var task = await db.Tasks.FirstOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        if (task is null) return NotFound();

        task.Title = title;
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToResponse(task));
    }

    private static async Task<IResult> UpdateStatus(string id, StatusRequest? request, AppDbContext db, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var taskId)) return BadRequest("Task ID must be a positive integer.");
        if (request?.Status is not ("Pending" or "Completed"))
            return BadRequest("Status must be Pending or Completed.");

        var task = await db.Tasks.FirstOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        if (task is null) return NotFound();

        task.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToResponse(task));
    }

    private static async Task<IResult> DeleteTask(string id, AppDbContext db, CancellationToken cancellationToken)
    {
        if (!TryParseId(id, out var taskId)) return BadRequest("Task ID must be a positive integer.");
        var task = await db.Tasks.FirstOrDefaultAsync(item => item.Id == taskId, cancellationToken);
        if (task is null) return NotFound();

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static bool TryParseId(string value, out int id) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;

    private static bool TryNormalizeTitle(string? value, out string title)
    {
        title = value?.Trim() ?? string.Empty;
        return title.Length is > 0 and <= 255;
    }

    private static TaskResponse ToResponse(TaskItem task) => new(task.Id, task.Title, task.Status);
    private static IResult BadRequest(string detail) => Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: detail);
    private static IResult NotFound() => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Task not found", detail: "No task exists with that ID.");
}
