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
            _projectIdTask ??= github.Connection.FetchProjectIdAsync(owner, number);

            _projectId = await _projectIdTask;
        }

        return _projectId;
    }

    public async Task<IReadOnlyList<GitHubProjectItem>> GetAllItemsDetailedAsync(bool includeClosed = false)
    {
        var items = await GetAllItemsAsync(includeClosed);

        await Parallel.ForEachAsync(items, async (item, ct) =>
        {
            if (item.Content is GitHubIssue issue)
            {
                var repo = github.GetRepository(issue.Owner, issue.Repository);
                await repo.GetIssueDetailedAsync(issue.Number);
            }

            // TODO: PRs
            // TODO: draft issues
        });

        return items;
    }

    public async Task<IReadOnlyList<GitHubProjectItem>> GetAllItemsAsync(bool includeClosed = false)
    {
        var projectId = await GetProjectIdAsync();

        var items = await github.Connection.FetchAllProjectItemsAsync(projectId);

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
}
