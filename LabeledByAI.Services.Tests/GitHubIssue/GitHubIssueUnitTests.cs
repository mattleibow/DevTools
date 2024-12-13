namespace LabeledByAI.Services.Tests;

public partial class GitHubIssueUnitTests
{
    private static GitHubIssue CreateIssue(
        string id = "issue_id",
        string owner = "owner_name",
        string repository = "repo_name",
        int number = 1,
        bool isOpen = true,
        string author = "author_name",
        string title = "Issue Title",
        string body = "Issue Body",
        int? totalComments = null,
        int? totalReactions = null,
        DateTimeOffset? lastActivityOn = default,
        DateTimeOffset? createdOn = default,
        IReadOnlyList<string>? labels = default,
        IReadOnlyList<GitHubComment>? comments = default,
        IReadOnlyList<GitHubReaction>? reactions = default)
    {
        var now = DateTimeOffset.Now;

        var issue = new GitHubIssue(
            id,
            owner,
            repository,
            number,
            isOpen,
            author,
            title,
            body,
            totalComments ?? comments?.Count ?? 0,
            totalReactions ?? reactions?.Count ?? 0,
            lastActivityOn ?? DateTimeOffset.Now,
            createdOn ?? now,
            labels ?? [])
        {
            Comments = comments,
            Reactions = reactions
        };

        return issue;
    }
}
