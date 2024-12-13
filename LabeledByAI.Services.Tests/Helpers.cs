namespace LabeledByAI.Services.Tests;

public static class Helpers
{
    public static readonly DateTimeOffset FakeNow = DateTimeOffset.Now;

    public static GitHubIssue CreateIssue(
       string? id = null,
       string owner = "owner",
       string repository = "repo",
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
            repository,
            number,
            isOpen,
            author,
            title,
            body,
            totalComments ?? comments?.Count ?? 0,
            totalReactions ?? 0,
            lastActivityOn ?? FakeNow,
            createdOn ?? FakeNow,
            labels ?? [])
        {
            Comments = comments,
        };

        return issue;
    }

    public static GitHubComment CreateComment(
       string id = "comment_id",
       string author = "author_name",
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
}
