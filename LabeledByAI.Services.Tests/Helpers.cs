namespace LabeledByAI.Services.Tests;

public static class Helpers
{
    public static readonly DateTimeOffset FakeNow = DateTimeOffset.Now;
    public static readonly DateTimeOffset FakeLastWeek = DateTimeOffset.Now.AddDays(-7);
    public static readonly DateTimeOffset FakeLastMonth = DateTimeOffset.Now.AddMonths(-1);

    public static GitHubIssue CreateIssue(
       string? id = null,
       string owner = "owner",
       string repo = "repo",
       int number = 1,
       bool isOpen = true,
       string author = "author",
       string title = "Issue Title",
       string body = "Issue Body",
       int? totalComments = null,
       int? totalReactions = null,
       DateTimeOffset? lastActivityOn = default,
       DateTimeOffset? createdOn = default,
       IReadOnlyList<string>? labels = default,
       IReadOnlyList<GitHubComment>? comments = default)
    {
        var issue = new GitHubIssue(
            id ?? $"issue_{number}",
            owner,
            repo,
            number,
            isOpen,
            author,
            title,
            body,
            totalComments ?? comments?.Count ?? 0,
            totalReactions ?? 0,
            lastActivityOn ?? comments?.Max(c => c.CreatedOn) ?? createdOn ?? FakeNow,
            createdOn ?? FakeNow,
            labels ?? [])
        {
            Comments = comments,
        };

        return issue;
    }

    public static GitHubComment CreateComment(
       string id = "comment_id",
       string author = "author",
       bool isBot = false,
       string body = "Issue Body",
       DateTimeOffset? createdOn = default,
       int? totalReactions = null)
    {
        var comment = new GitHubComment(
            id,
            author,
            isBot ? $"/apps/{author}" : $"/{author}",
            body,
            createdOn ?? FakeNow,
            totalReactions ?? 0);

        return comment;
    }

    public static GitHubReaction CreateReaction(
       string id = "reaction_id",
       string author = "author",
       string reaction = "thumbs_up",
       DateTimeOffset? createdOn = default)
    {
        var reactionObj = new GitHubReaction(
            id,
            author,
            reaction,
            createdOn ?? FakeNow);

        return reactionObj;
    }
}
