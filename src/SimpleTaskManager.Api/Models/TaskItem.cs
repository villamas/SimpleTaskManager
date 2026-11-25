namespace SimpleTaskManager.Api.Models;

public sealed class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string Status { get; set; } = "Pending";
}
