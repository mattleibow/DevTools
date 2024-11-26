namespace LabeledByAI.Services;

public record GitHubIssue(
    string Id,
    int Number,
    string Title,
    string Body);
