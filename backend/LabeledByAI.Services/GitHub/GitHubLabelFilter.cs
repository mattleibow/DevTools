namespace LabeledByAI.Services;

public record GitHubLabelFilter(
    string[]? Names = null,
    string? Pattern = null);
