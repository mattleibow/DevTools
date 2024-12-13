namespace LabeledByAI.Services;

public record GitHubReaction(
    string Id,
    string Author,
    string Rection,
    DateTimeOffset CreatedOn);
