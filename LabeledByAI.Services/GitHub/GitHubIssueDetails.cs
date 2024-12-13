namespace LabeledByAI.Services;

public record GitHubIssueDetails(
    IReadOnlyList<GitHubComment> Comments,
    IReadOnlyList<GitHubReaction> Reactions);
