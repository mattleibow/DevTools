using FluentAssertions;
using NSubstitute;

namespace LabeledByAI.Services.Tests;

public partial class GitHubRepositoryUnitTests
{
    private const string TestOwner = "mattleibow";
    private const string TestRepo = "fake-repo";

    private static List<GitHubIssue> BasicExpectedIssues => [
        Helpers.CreateIssue(number: 1, title: "First Issue", totalComments: 2, totalReactions: 3),
        Helpers.CreateIssue(number: 2, title: "Second Issue", isOpen: false, totalComments: 3),
        Helpers.CreateIssue(number: 3, title: "Third Issue", totalComments: 0, totalReactions: 1),
    ];

    private static Dictionary<int, GitHubIssueDetails> BasicExpectedDetails => new()
    {
        [1] = new([
                Helpers.CreateComment(body: "First Issue Comment 1"),
                Helpers.CreateComment(body: "First Issue Comment 2"),
            ], [
                Helpers.CreateReaction(),
                Helpers.CreateReaction(),
                Helpers.CreateReaction()
            ]),
        [2] = new([
                Helpers.CreateComment(body: "Second Issue Comment 1"),
                Helpers.CreateComment(body: "Second Issue Comment 2"),
                Helpers.CreateComment(body: "Second Issue Comment 3"),
            ], [
                // No reactions
            ]),
        [3] = new([
                // No comments
            ], [
                Helpers.CreateReaction()
            ]),
    };

    private static GitHubRepository GetRepo(List<GitHubIssue> issues)
    {
        var connection = GetConnection(issues);

        var github = new GitHub(connection);
        var repo = github.GetRepository(TestOwner, TestRepo);

        return repo;
    }

    private static GitHubRepository GetRepo(
        List<GitHubIssue> issues,
        Dictionary<int, GitHubIssueDetails> details)
    {
        var connection = GetConnection(issues);

        foreach (var issue in issues)
        {
            connection.FetchIssueDetailsAsync(TestOwner, TestRepo, issue.Number)
                .Returns(details[issue.Number]);
        }

        var github = new GitHub(connection);
        var repo = github.GetRepository("mattleibow", "fake-repo");

        return repo;
    }

    private static IGitHubConnection GetConnection(List<GitHubIssue> issues)
    {
        var connection = Substitute.For<IGitHubConnection>();

        connection.FetchAllIssuesAsync("mattleibow", "fake-repo")
            .Returns(issues.Where(i => i.IsOpen).ToList());

        connection.FetchAllIssuesAsync("mattleibow", "fake-repo", true)
            .Returns(issues);
        return connection;
    }
}
