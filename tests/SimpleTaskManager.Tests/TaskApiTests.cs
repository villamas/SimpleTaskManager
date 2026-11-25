using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleTaskManager.Api.Contracts;
using SimpleTaskManager.Api.Data;
using Xunit;

namespace SimpleTaskManager.Tests;

public sealed class TaskApiTests
{
    [Fact]
    public async Task IT001_CreateAndRetrieveTask_ReturnsIdAndPendingStatus()
    {
        using var database = new TestDatabase();
        using var factory = new TestApplicationFactory(database.FilePath);
        using var client = factory.CreateClient();

        var created = await CreateTask(client, "First synthetic task");
        var retrieved = await client.GetFromJsonAsync<TaskResponse>($"/api/tasks/{created.Id}");

        Assert.Equal(created.Id, retrieved?.Id);
        Assert.Equal("First synthetic task", retrieved?.Title);
        Assert.Equal("Pending", retrieved?.Status);
    }

    [Fact]
    public async Task IT002_InvalidCreateAndEditTitles_AreRejectedWithoutChangingData()
    {
        using var database = new TestDatabase();
        using var factory = new TestApplicationFactory(database.FilePath);
        using var client = factory.CreateClient();
        var existing = await CreateTask(client, "Original title");

        foreach (var invalidTitle in new[] { "", "   ", new string('x', 256) })
        {
            using var createResponse = await client.PostAsJsonAsync("/api/tasks", new TitleRequest(invalidTitle));
            using var editResponse = await client.PatchAsJsonAsync($"/api/tasks/{existing.Id}", new TitleRequest(invalidTitle));

            Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, editResponse.StatusCode);
        }

        var tasks = await client.GetFromJsonAsync<TaskResponse[]>("/api/tasks?status=All");
        Assert.Collection(tasks!, task =>
        {
            Assert.Equal(existing.Id, task.Id);
            Assert.Equal("Original title", task.Title);
            Assert.Equal("Pending", task.Status);
        });
    }

    [Fact]
    public async Task IT003_UpdateTitle_PreservesIdAndStatus()
    {
        using var database = new TestDatabase();
        using var factory = new TestApplicationFactory(database.FilePath);
        using var client = factory.CreateClient();
        var created = await CreateTask(client, "Before update");
        using var statusResponse = await client.PutAsJsonAsync($"/api/tasks/{created.Id}/status", new StatusRequest("Completed"));
        statusResponse.EnsureSuccessStatusCode();

        using var updateResponse = await client.PatchAsJsonAsync($"/api/tasks/{created.Id}", new TitleRequest("After update"));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskResponse>();

        Assert.Equal(created.Id, updated?.Id);
        Assert.Equal("After update", updated?.Title);
        Assert.Equal("Completed", updated?.Status);
    }

    [Fact]
    public async Task IT004_DeleteTask_RemovesIt()
    {
        using var database = new TestDatabase();
        using var factory = new TestApplicationFactory(database.FilePath);
        using var client = factory.CreateClient();
        var created = await CreateTask(client, "Task to delete");

        using var deleteResponse = await client.DeleteAsync($"/api/tasks/{created.Id}");
        using var getResponse = await client.GetAsync($"/api/tasks/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task IT005_StatusCanChangeInBothDirections()
    {
        using var database = new TestDatabase();
        using var factory = new TestApplicationFactory(database.FilePath);
        using var client = factory.CreateClient();
        var created = await CreateTask(client, "Status transition task");

        using var completedResponse = await client.PutAsJsonAsync($"/api/tasks/{created.Id}/status", new StatusRequest("Completed"));
        var completed = await completedResponse.Content.ReadFromJsonAsync<TaskResponse>();
        using var pendingResponse = await client.PutAsJsonAsync($"/api/tasks/{created.Id}/status", new StatusRequest("Pending"));
        var pending = await pendingResponse.Content.ReadFromJsonAsync<TaskResponse>();

        Assert.Equal(HttpStatusCode.OK, completedResponse.StatusCode);
        Assert.Equal("Completed", completed?.Status);
        Assert.Equal(HttpStatusCode.OK, pendingResponse.StatusCode);
        Assert.Equal("Pending", pending?.Status);
    }

    [Fact]
    public async Task IT006_StatusFiltersKeepCreationOrderAndDoNotChangeTasks()
    {
        using var database = new TestDatabase();
        using var factory = new TestApplicationFactory(database.FilePath);
        using var client = factory.CreateClient();
        var first = await CreateTask(client, "First pending");
        var second = await CreateTask(client, "Completed task");
        using var statusResponse = await client.PutAsJsonAsync($"/api/tasks/{second.Id}/status", new StatusRequest("Completed"));
        statusResponse.EnsureSuccessStatusCode();
        var third = await CreateTask(client, "Second pending");

        var all = await client.GetFromJsonAsync<TaskResponse[]>("/api/tasks");
        var pending = await client.GetFromJsonAsync<TaskResponse[]>("/api/tasks?status=Pending");
        var completed = await client.GetFromJsonAsync<TaskResponse[]>("/api/tasks?status=Completed");
        var allAfterFilters = await client.GetFromJsonAsync<TaskResponse[]>("/api/tasks?status=All");
        var allTasks = all!;
        var tasksAfterFilters = allAfterFilters!;

        Assert.Equal(new[] { first.Id, second.Id, third.Id }, allTasks.Select(task => task.Id));
        Assert.Equal(new[] { first.Id, third.Id }, pending!.Select(task => task.Id));
        Assert.Equal(new[] { second.Id }, completed!.Select(task => task.Id));
        Assert.Equal(allTasks.Select(task => (task.Id, task.Title, task.Status)),
            tasksAfterFilters.Select(task => (task.Id, task.Title, task.Status)));
    }

    [Fact]
    public async Task IT007_NonexistentTaskOperationsReturnNotFoundWithoutChangingOtherTasks()
    {
        using var database = new TestDatabase();
        using var factory = new TestApplicationFactory(database.FilePath);
        using var client = factory.CreateClient();
        var existing = await CreateTask(client, "Unaffected task");
        const int missingId = 999_999;

        using var getResponse = await client.GetAsync($"/api/tasks/{missingId}");
        using var editResponse = await client.PatchAsJsonAsync($"/api/tasks/{missingId}", new TitleRequest("Changed"));
        using var statusResponse = await client.PutAsJsonAsync($"/api/tasks/{missingId}/status", new StatusRequest("Completed"));
        using var deleteResponse = await client.DeleteAsync($"/api/tasks/{missingId}");
        var tasks = await client.GetFromJsonAsync<TaskResponse[]>("/api/tasks?status=All");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, editResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, statusResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Collection(tasks!, task =>
        {
            Assert.Equal(existing.Id, task.Id);
            Assert.Equal("Unaffected task", task.Title);
            Assert.Equal("Pending", task.Status);
        });
    }

    [Fact]
    public async Task IT008_TaskChangesPersistWhenHostRestartsWithTheSameDatabase()
    {
        using var database = new TestDatabase();
        int retainedTaskId;
        int deletedTaskId;

        using (var factory = new TestApplicationFactory(database.FilePath))
        using (var client = factory.CreateClient())
        {
            var retained = await CreateTask(client, "Persisted task");
            retainedTaskId = retained.Id;
            using var editResponse = await client.PatchAsJsonAsync($"/api/tasks/{retained.Id}", new TitleRequest("Edited persisted task"));
            editResponse.EnsureSuccessStatusCode();
            using var statusResponse = await client.PutAsJsonAsync($"/api/tasks/{retained.Id}/status", new StatusRequest("Completed"));
            statusResponse.EnsureSuccessStatusCode();

            var toDelete = await CreateTask(client, "Deleted before restart");
            deletedTaskId = toDelete.Id;
            using var deleteResponse = await client.DeleteAsync($"/api/tasks/{toDelete.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        using (var factory = new TestApplicationFactory(database.FilePath))
        using (var client = factory.CreateClient())
        {
            var persisted = await client.GetFromJsonAsync<TaskResponse>($"/api/tasks/{retainedTaskId}");
            using var deletedResponse = await client.GetAsync($"/api/tasks/{deletedTaskId}");

            Assert.Equal("Edited persisted task", persisted?.Title);
            Assert.Equal("Completed", persisted?.Status);
            Assert.Equal(HttpStatusCode.NotFound, deletedResponse.StatusCode);
        }
    }

    private static async Task<TaskResponse> CreateTask(HttpClient client, string title)
    {
        using var response = await client.PostAsJsonAsync("/api/tasks", new TitleRequest(title));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<TaskResponse>())!;
    }

    private sealed class TestApplicationFactory(string databasePath) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
                services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={databasePath};Pooling=False"));
            });
        }
    }

    private sealed class TestDatabase : IDisposable
    {
        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), $"SimpleTaskManager.Tests-{Guid.NewGuid():N}");
        public string FilePath => Path.Combine(DirectoryPath, "tasks.db");

        public TestDatabase() => Directory.CreateDirectory(DirectoryPath);

        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath)) Directory.Delete(DirectoryPath, recursive: true);
        }
    }
}
