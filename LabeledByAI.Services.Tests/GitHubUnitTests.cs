using FluentAssertions;
using NSubstitute;

namespace LabeledByAI.Services.Tests;

public class GitHubUnitTests
{
    public const string TestOwner = "mattleibow";
    public const string TestRepo = "fake-repo";
    public const string TestProject = "project_1";
    public const int TestProjectNumber = 1;

    public static List<GitHubIssue> BasicExpectedIssues => [
        Helpers.CreateIssue(owner: TestOwner, repo: TestRepo, number: 1, title: "First Issue", totalComments: 2, totalReactions: 3, createdOn: Helpers.FakeLastWeek),
        Helpers.CreateIssue(owner: TestOwner, repo: TestRepo, number: 2, title: "Second Issue", isOpen: false, totalComments: 3),
        Helpers.CreateIssue(owner: TestOwner, repo: TestRepo, number: 3, title: "Third Issue", totalComments: 0, totalReactions: 1),
    ];

    public static Dictionary<int, GitHubIssueDetails> BasicExpectedDetails => new()
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

    public static List<GitHubProjectItem> BasicExpectedProjectItems =>
    [
        new ("item_1", GitHubProjectItemType.Issue, BasicExpectedIssues[0]),
        new ("item_2", GitHubProjectItemType.Issue, BasicExpectedIssues[1]),
        new ("item_3", GitHubProjectItemType.Issue, BasicExpectedIssues[2]),
    ];

    public static IGitHubConnection GetConnection(
        List<GitHubProjectItem>? projectItems = null,
        List<GitHubIssue>? issues = null,
        Dictionary<int, GitHubIssueDetails>? details = null)
    {
        var connection = Substitute.For<IGitHubConnection>();

        if (projectItems is not null)
        {
            AddProject(connection);
            AddProjectItems(connection, projectItems);
        }

        if (issues is not null)
        {
            AddIssues(connection, issues);
            if (details is not null)
            {
                AddIssueDetails(connection, issues, details);
            }
        }

        return connection;
    }

    public static GitHubRepository GetRepo(List<GitHubIssue> issues)
    {
        var connection = GetConnection(
            issues: issues);

        var github = new GitHub(connection);
        var repo = github.GetRepository(TestOwner, TestRepo);

        return repo;
    }

    public static GitHubRepository GetRepo(
        List<GitHubIssue> issues,
        Dictionary<int, GitHubIssueDetails> details)
    {
        var connection = GetConnection(
            issues: issues,
            details: details);

        var github = new GitHub(connection);
        var repo = github.GetRepository(TestOwner, TestRepo);

        return repo;
    }

    public static GitHubProject GetProject(
        List<GitHubProjectItem> projectItems,
        List<GitHubIssue> issues,
        Dictionary<int, GitHubIssueDetails> details)
    {
        var connection = GetConnection(
            projectItems: projectItems,
            issues: issues,
            details: details);

        var github = new GitHub(connection);
        var project = github.GetProject(TestOwner, TestProjectNumber);

        return project;
    }

    public static GitHubProject GetProject(
        List<GitHubProjectItem> projectItems,
        List<GitHubIssue> issues)
    {
        var connection = GetConnection(
            projectItems: projectItems,
            issues: issues);

        var github = new GitHub(connection);
        var project = github.GetProject(TestOwner, TestProjectNumber);

        return project;
    }

    private static void AddProject(
        IGitHubConnection connection)
    {
        connection.FetchProjectIdAsync(TestOwner, TestProjectNumber)
            .Returns(TestProject);
    }

    private static void AddProjectItems(
        IGitHubConnection connection,
        List<GitHubProjectItem> items)
    {
        connection.FetchAllProjectItemsAsync(TestProject)
            .Returns(items);
    }

    private static void AddIssues(
        IGitHubConnection connection,
        List<GitHubIssue> issues)
    {
        foreach (var issue in issues)
        {
            connection.FetchIssueAsync(TestOwner, TestRepo, issue.Number)
                .Returns(issue);
        }

        connection.FetchAllIssuesAsync(TestOwner, TestRepo)
            .Returns(issues.Where(i => i.IsOpen).ToList());

        connection.FetchAllIssuesAsync(TestOwner, TestRepo, true)
            .Returns(issues);
    }

    private static void AddIssueDetails(
        IGitHubConnection connection,
        List<GitHubIssue> issues,
        Dictionary<int, GitHubIssueDetails> details)
    {
        foreach (var issue in issues)
        {
            connection.FetchIssueDetailsAsync(TestOwner, TestRepo, issue.Number)
                .Returns(details[issue.Number]);
        }

        foreach (var detail in details)
        {
            connection.FetchIssueCommentsAsync(TestOwner, TestRepo, detail.Key)
                .Returns(detail.Value.Comments);
        }
    }
}
