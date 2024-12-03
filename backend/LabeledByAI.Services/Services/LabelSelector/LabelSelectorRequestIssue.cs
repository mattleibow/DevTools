namespace LabeledByAI.Services;

public record LabelSelectorRequestIssue(
    string Owner,
    string Repo,
    int Number);
