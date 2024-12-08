using Octokit.GraphQL;
using Octokit.GraphQL.Model;

namespace LabeledByAI.Services;

public class GitHubProject(GitHub github, string owner, int number)
{
    private string? _projectId;
    private Task<string>? _projectIdTask;
    private readonly Dictionary<string, GitHubProjectItem> _allItems = [];

    public async Task<string> GetProjectIdAsync()
    {
        if (string.IsNullOrEmpty(_projectId))
        {
            _projectIdTask ??= FetchProjectIdAsync();

            _projectId = await _projectIdTask;
        }

        return _projectId;
    }

    public async Task<IList<GitHubProjectItem>> GetAllItemsDetailedAsync(bool includeClosed = false)
    {
        var items = await GetAllItemsAsync(includeClosed);

        foreach (var item in items)
        {
            if (item.Content is GitHubIssue issue)
            {
                var repo = github.GetRepository(issue.Owner, issue.Repository);
                await repo.GetIssueDetailedAsync(issue.Number);
            }

            // TODO: PRs
            // TODO: draft issues
        }

        return items;
    }

    public async Task<IList<GitHubProjectItem>> GetAllItemsAsync(bool includeClosed = false)
    {
        var projectId = await GetProjectIdAsync();

        var items = await FetchAllItemsAsync(projectId);

        foreach (var item in items)
        {
            _allItems[item.Id] = item;

            if (item.Content is GitHubIssue issue)
            {
                var repo = github.GetRepository(issue.Owner, issue.Repository);
                repo.CacheIssue(issue);
            }

            // TODO: PRs
            // TODO: draft issues
        }

        if (!includeClosed)
        {
            items = items
                .Where(i => i.Content is not GitHubIssue issue || issue.IsOpen)
                // TODO: PRs
                // TODO: draft issues
                .ToList();
        }

        return items;
    }

    private async Task<string> FetchProjectIdAsync()
    {
        string projectId;
        try
        {
            var orgQuery = new Query()
                .Organization(owner)
                .ProjectV2(number)
                .Select(project => project.Id.Value)
                .Compile();

            projectId = await github.Connection.Run(orgQuery);
        }
        catch (Exception ex)
        {
            var userQuery = new Query()
                .User(owner)
                .ProjectV2(number)
                .Select(project => project.Id.Value)
                .Compile();

            projectId = await github.Connection.Run(userQuery);
        }

        return projectId;
    }

    private async Task<IList<GitHubProjectItem>> FetchAllItemsAsync(string projectId)
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
                    )
                    // TODO: PRs
                    // TODO: draft issues
                ))
                .ToList()
            )
            .Compile();

        var items = await github.Connection.Run(query);

        return items.ToList();
    }
}
