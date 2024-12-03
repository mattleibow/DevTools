namespace LabeledByAI.Services;

public record EngagementRequestIssue(
    string Owner,
    string Repo,
    int? Number);
