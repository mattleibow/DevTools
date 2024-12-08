namespace LabeledByAI.Services;

public record GitHubProjectItem(
    string Id,
    GitHubProjectItemType Type,
    GitHubIssue Content);
