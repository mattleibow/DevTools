namespace LabeledByAI;

public record NewIssue(
    string[] Labels,
    string Body,
    string? Title = null,
    string? Url = null);
