namespace LabeledByAI.Services;

public record GitHubComment(
    string Id,
    string Author,
    string AuthorType,
    string Body,
    DateTimeOffset CreatedOn,
    int TotalReactions)
{
    public IReadOnlyList<GitHubReaction>? Reactions { get; internal set; }

    public bool IsUser =>
        !AuthorType.StartsWith("/apps/");
}
