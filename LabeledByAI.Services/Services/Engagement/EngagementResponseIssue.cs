namespace LabeledByAI.Services;

public record EngagementResponseIssue(
    string Id,
    string Owner,
    string Repo,
    int Number);
