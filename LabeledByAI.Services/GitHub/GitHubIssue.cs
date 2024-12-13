using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace LabeledByAI.Services;

public record GitHubIssue(
    string Id,
    string Owner,
    string Repository,
    int Number,
    bool IsOpen,
    string Author,
    string Title,
    string Body,
    int TotalComments,
    int TotalReactions,
    DateTimeOffset LastActivityOn,
    DateTimeOffset CreatedOn,
    IReadOnlyList<string> Labels)
{
    public IReadOnlyList<GitHubComment>? Comments { get; internal set; }

    public IReadOnlyList<GitHubReaction>? Reactions { get; internal set; }

    [JsonIgnore]
    public IEnumerable<GitHubComment>? UserComments =>
        Comments?.Where(c => c.IsUser);

    public int TotalUserComments =>
        UserComments?.Count() ?? 0;

    public int TotalCommentReactions =>
        Comments?.Sum(c => c.TotalReactions) ?? 0;

    public IEnumerable<string> UserContributors =>
        UserComments?.Select(c => c.Author)?.Union([Author]).Distinct() ?? [Author];

    public int TotalUserContributors =>
        UserContributors.Count();

    [JsonIgnore]
    public TimeSpan Age =>
        DateTimeOffset.UtcNow - CreatedOn.ToUniversalTime();

    [JsonIgnore]
    public TimeSpan TimeSinceLastActivity =>
        DateTimeOffset.UtcNow - LastActivityOn.ToUniversalTime();

    public bool TryGetHistoricIssue(DateTimeOffset lastActivityOn, [NotNullWhen(true)] out GitHubIssue? historic)
    {
        // fail fast if the issue was created after the date we want
        if (CreatedOn > lastActivityOn)
        {
            historic = null;
            return false;
        }

        // get comments and comment reactions
        var comments = Comments?
            .Where(c => c.CreatedOn <= lastActivityOn)
            .Select(c => c with
            {
                Reactions = c.Reactions
                    .Where(r => r.CreatedOn <= lastActivityOn)
                    .ToList()
            })
            .ToList();

        // get issue reactions
        var reactions = Reactions?
            .Where(r => r.CreatedOn <= lastActivityOn)
            .ToList();

        var oldIssue = this with
        {
            Comments = comments,
            Reactions = reactions,
            LastActivityOn = lastActivityOn
        };

        historic = oldIssue;
        return true;
    }
}
