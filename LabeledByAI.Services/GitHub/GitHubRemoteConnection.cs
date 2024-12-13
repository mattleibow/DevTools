using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace LabeledByAI.Services;

public class GitHubRemoteConnection : IGitHubConnection
{
    private const string ApplicationName = "labeled-by-ai";
    private const string ApplicationVersion = "1.0";

    private Connection? _connection;

    public void SetToken(string githubToken)
    {
        _connection = new Connection(
            new ProductHeaderValue(ApplicationName, ApplicationVersion),
            githubToken);
    }

    public Connection Connection =>
        _connection ?? throw new InvalidOperationException("GitHub token not set.");

    public async Task<IReadOnlyList<GitHubComment>> FetchIssueCommentsAsync(string owner, string repo, int number)
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issue(number: number)
            .Select(i =>
                i.Comments(null, null, null, null, null)
                    .AllPages()
                    .Select(c => new GitHubComment(
                        c.Id.ToString(),
                        c.Author == null ? "ghost" : c.Author.Login,
                        c.Author == null ? "/ghost" : c.Author.ResourcePath,
                        c.Body,
                        c.CreatedAt,
                        c.Reactions(null, null, null, null, null, null)
                            .TotalCount
                    ))
                    .ToList()
            )
            .Compile();

        var comments = await Connection.Run(query);

        return comments;
    }

    public async Task<GitHubIssueDetails> FetchIssueDetailsAsync(string owner, string repo, int number)
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issue(number: number)
            .Select(i => new GitHubIssueDetails(
                i.Comments(null, null, null, null, null)
                    .AllPages()
                    .Select(c => new GitHubComment(
                        c.Id.ToString(),
                        c.Author == null ? "ghost" : c.Author.Login,
                        c.Author == null ? "/ghost" : c.Author.ResourcePath,
                        c.Body,
                        c.CreatedAt,
                        c.Reactions(null, null, null, null, null, null)
                            .TotalCount)
                    {
                        Reactions = c.Reactions(null, null, null, null, null, null)
                            .AllPages()
                            .Select(r => new GitHubReaction(
                                r.Id.ToString(),
                                r.User == null ? "ghost" : r.User.Login,
                                r.Content.ToString(),
                                r.CreatedAt
                            ))
                            .ToList()
                    })
                    .ToList(),
                i.Reactions(null, null, null, null, null, null)
                    .AllPages()
                    .Select(r => new GitHubReaction(
                        r.Id.ToString(),
                        r.User == null ? "ghost" : r.User.Login,
                        r.Content.ToString(),
                        r.CreatedAt
                    ))
                    .ToList()
            ))
            .Compile();

        var details = await Connection.Run(query);

        return details;
    }

    public async Task<GitHubIssueDetails> FetchIssueDetailsAsync(string owner, string repo, int number)
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issue(number: number)
            .Select(i => new GitHubIssueDetails(
                i.Comments(null, null, null, null, null)
                    .AllPages()
                    .Select(c => new GitHubComment(
                        c.Id.ToString(),
                        c.Author == null ? "ghost" : c.Author.Login,
                        c.Author == null ? "/ghost" : c.Author.ResourcePath,
                        c.Body,
                        c.CreatedAt,
                        c.Reactions(null, null, null, null, null, null)
                            .TotalCount)
                    {
                        Reactions = c.Reactions(null, null, null, null, null, null)
                            .AllPages()
                            .Select(r => new GitHubReaction(
                                r.Id.ToString(),
                                r.User == null ? "ghost" : r.User.Login,
                                r.Content.ToString(),
                                r.CreatedAt
                            ))
                            .ToList()
                    })
                    .ToList(),
                i.Reactions(null, null, null, null, null, null)
                    .AllPages()
                    .Select(r => new GitHubReaction(
                        r.Id.ToString(),
                        r.User == null ? "ghost" : r.User.Login,
                        r.Content.ToString(),
                        r.CreatedAt
                    ))
                    .ToList()
            ))
            .Compile();

        var details = await Connection.Run(query);

        return details;
    }

    public async Task<IReadOnlyList<GitHubLabel>> FetchLabelsAsync(string owner, string repo)
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Labels()
            .AllPages()
            .Select(label => new GitHubLabel(
                label.Id.ToString(),
                label.Name,
                label.Description,
                label.Issues(null, null, null, null, null, null, null, null)
                    .TotalCount
            ))
            .Compile();

        var labels = await Connection.Run(query);

        return labels.ToList();
    }

    public async Task<GitHubIssue> FetchIssueAsync(string owner, string repo, int number)
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issue(number: number)
            .Select(i => new GitHubIssue(
                i.Id.ToString(),
                i.Repository.Owner.Login,
                i.Repository.Name,
                i.Number,
                i.State == IssueState.Open,
                i.Author.Login,
                i.Title,
                i.Body,
                i.Comments(null, null, null, null, null)
                    .TotalCount,
                i.Reactions(null, null, null, null, null, null)
                    .TotalCount,
                i.UpdatedAt,
                i.CreatedAt,
                i.Labels(null, null, null, null, null)
                    .AllPages()
                    .Select(l => l.Name)
                    .ToList()
            ))
            .Compile();

        var issue = await Connection.Run(query);

        return issue;
    }

    public async Task<IReadOnlyList<GitHubIssue>> FetchAllIssuesAsync(string owner, string repo, bool includeClosed = false)
    {
        IssueState[] issueStates = includeClosed
            ? [IssueState.Open, IssueState.Closed]
            : [IssueState.Open];

        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issues(states: issueStates)
            .AllPages()
            .Select(i => new GitHubIssue(
                i.Id.ToString(),
                i.Repository.Owner.Login,
                i.Repository.Name,
                i.Number,
                i.State == IssueState.Open,
                i.Author.Login,
                i.Title,
                i.Body,
                i.Comments(null, null, null, null, null)
                    .TotalCount,
                i.Reactions(null, null, null, null, null, null)
                    .TotalCount,
                i.UpdatedAt,
                i.CreatedAt,
                i.Labels(null, null, null, null, null)
                    .AllPages()
                    .Select(l => l.Name)
                    .ToList()
            ))
            .Compile();

        var issues = await Connection.Run(query);

        return issues.ToList();
    }

    public async Task<GitHubIssueDetails> FetchIssueDetailsAsync(string owner, string repo, int number)
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issue(number: number)
            .Select(i => new GitHubIssueDetails(
                i.Comments(null, null, null, null, null)
                    .AllPages()
                    .Select(c => new GitHubComment(
                        c.Id.ToString(),
                        c.Author == null ? "ghost" : c.Author.Login,
                        c.Author == null ? "/ghost" : c.Author.ResourcePath,
                        c.Body,
                        c.CreatedAt,
                        c.Reactions(null, null, null, null, null, null)
                            .TotalCount)
                    {
                        Reactions = c.Reactions(null, null, null, null, null, null)
                            .AllPages()
                            .Select(r => new GitHubReaction(
                                r.Id.ToString(),
                                r.User == null ? "ghost" : r.User.Login,
                                r.Content.ToString(),
                                r.CreatedAt
                            ))
                            .ToList()
                    })
                    .ToList(),
                i.Reactions(null, null, null, null, null, null)
                    .AllPages()
                    .Select(r => new GitHubReaction(
                        r.Id.ToString(),
                        r.User == null ? "ghost" : r.User.Login,
                        r.Content.ToString(),
                        r.CreatedAt
                    ))
                    .ToList()
            ))
            .Compile();

        var details = await Connection.Run(query);

        return details;
    }

    public async Task<string> FetchProjectIdAsync(string owner, int number)
    {
        string projectId;
        try
        {
            var orgQuery = new Query()
                .Organization(owner)
                .ProjectV2(number)
                .Select(project => project.Id.Value)
                .Compile();

            projectId = await Connection.Run(orgQuery);
        }
        catch (Exception ex)
        {
            var userQuery = new Query()
                .User(owner)
                .ProjectV2(number)
                .Select(project => project.Id.Value)
                .Compile();

            projectId = await Connection.Run(userQuery);
        }

        return projectId;
    }

    public async Task<IReadOnlyList<GitHubProjectItem>> FetchAllProjectItemsAsync(string projectId)
    {
        var query = new Query()
            .Node(new ID(projectId))
            .Cast<ProjectV2>()
            .Select(project => project
                .Items(null, null, null, null, null)
                .AllPages()
                .Select(item => new GitHubProjectItem(
                    item.Id.Value,
                    (GitHubProjectItemType)item.Type,
                    item.Content.Switch<GitHubIssue>(content => content
                        .Issue(i => new GitHubIssue(
                            i.Id.ToString(),
                            i.Repository.Owner.Login,
                            i.Repository.Name,
                            i.Number,
                            i.State == IssueState.Open,
                            i.Author == null ? "ghost" : i.Author.Login,
                            i.Title,
                            i.Body,
                            i.Comments(null, null, null, null, null)
                                .TotalCount,
                            i.Reactions(null, null, null, null, null, null)
                                .TotalCount,
                            i.UpdatedAt,
                            i.CreatedAt,
                            i.Labels(null, null, null, null, null)
                                .AllPages()
                                .Select(l => l.Name)
                                .ToList()
                        ))
                    )
                // TODO: PRs
                // TODO: draft issues
                ))
                .ToList()
            )
            .Compile();

        var items = await Connection.Run(query);

        return items.ToList();
    }
}
