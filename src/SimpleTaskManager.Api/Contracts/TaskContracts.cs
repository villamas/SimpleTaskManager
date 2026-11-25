namespace SimpleTaskManager.Api.Contracts;

public sealed record TaskResponse(int Id, string Title, string Status);
public sealed record TitleRequest(string? Title);
public sealed record StatusRequest(string? Status);
