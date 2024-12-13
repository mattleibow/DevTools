using FluentAssertions;
using NSubstitute;

namespace LabeledByAI.Services.Tests;

public partial class GitHubRepositoryUnitTests
{
    private const string TestOwner = "mattleibow";
    private const string TestRepo = "fake-repo";

    private static List<GitHubIssue> BasicExpectedIssues => [
        Helpers.CreateIssue(number: 1, title: "First Issue", totalComments: 2),
        Helpers.CreateIssue(number: 2, title: "Second Issue", isOpen: false, totalComments: 3),
        Helpers.CreateIssue(number: 3, title: "Third Issue", totalComments: 0),
    ];

    private static Dictionary<int, List<GitHubComment>> BasicExpectedComments => new()
    {
        [1] = [
            Helpers.CreateComment(body: "First Issue Comment 1"),
            Helpers.CreateComment(body: "First Issue Comment 2"),
        ],
        [2] = [
            Helpers.CreateComment(body: "Second Issue Comment 1"),
            Helpers.CreateComment(body: "Second Issue Comment 2"),
            Helpers.CreateComment(body: "Second Issue Comment 3"),
        ],
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
        Dictionary<int, List<GitHubComment>> comments)
    {
        var connection = GetConnection(issues);

        // add the comments to the issues
        foreach (var (num, commentList) in comments)
        {
            issues.Should().ContainSingle(i =>
                i.Number == num &&
                i.TotalComments == commentList.Count);

            connection.FetchIssueCommentsAsync("mattleibow", "fake-repo", num)
                .Returns(commentList);
        }

        // any left over issues have no comments
        foreach (var issue in issues)
        {
            if (!comments.ContainsKey(issue.Number))
            {
                issue.TotalComments.Should().Be(0);

                connection.FetchIssueCommentsAsync("mattleibow", "fake-repo", issue.Number)
                    .Returns([]);
            }
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
