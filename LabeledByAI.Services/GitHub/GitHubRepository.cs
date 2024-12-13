using System.Text.RegularExpressions;

namespace LabeledByAI.Services;

public class GitHubRepository(GitHub github, string owner, string repo)
{
    private IReadOnlyList<GitHubLabel>? _allLabels;
    private readonly Dictionary<int, GitHubIssue> _allIssues = [];

    public GitHub GitHub => github;

    public async Task<IReadOnlyList<GitHubLabel>> GetLabelsAsync(GitHubLabelFilter? filter = null)
    {
        _allLabels ??= await github.Connection.FetchLabelsAsync(owner, repo);

        if (filter is null)
        {
            return _allLabels.ToList();
        }

        return GetFilteredLabels(filter);
    }

    public async Task<GitHubIssue> GetIssueAsync(int number)
    {
        if (!_allIssues.TryGetValue(number, out var issue))
        {
            issue = await github.Connection.FetchIssueAsync(owner, repo, number);
            _allIssues[number] = issue;
        }

        return issue;
    }

    public async Task<GitHubIssue> GetIssueDetailedAsync(int number)
    {
        var issue = await GetIssueAsync(number);

        await FetchAndApplyIssueDetailsAsync(github, owner, repo, issue);

        return issue;
    }

    public async Task<IReadOnlyList<GitHubIssue>> GetAllIssuesAsync(bool includeClosed = false)
    {
        var issues = await github.Connection.FetchAllIssuesAsync(owner, repo, includeClosed);

        foreach (var issue in issues)
        {
            _allIssues[issue.Number] = issue;
        }

        return issues;
    }

    public async Task<IReadOnlyList<GitHubIssue>> GetAllIssuesDetailedAsync(bool includeClosed = false)
    {
        var issues = await GetAllIssuesAsync(includeClosed);

        await Parallel.ForEachAsync(issues, async (issue, _) =>
        {
            await FetchAndApplyIssueDetailsAsync(github, owner, repo, issue);
        });

        return issues;
    }

    private static async Task FetchAndApplyIssueDetailsAsync(GitHub github, string owner, string repo, GitHubIssue issue)
    {
        if ((issue.Comments is null && issue.TotalComments > 0) ||
            (issue.Reactions is null && issue.TotalReactions > 0))
        {
            var details = await github.Connection.FetchIssueDetailsAsync(owner, repo, issue.Number);

            issue.Comments = details.Comments;
            issue.Reactions = details.Reactions;
        }
    }

    internal void CacheIssue(GitHubIssue issue)
    {
        if (_allIssues.TryGetValue(issue.Number, out var oldIssue))
        {
            issue.Comments ??= oldIssue.Comments;
        }

        _allIssues[issue.Number] = issue;
    }

    private List<GitHubLabel> GetFilteredLabels(GitHubLabelFilter filter)
    {
        if (_allLabels is null)
            throw new InvalidOperationException("Trying to filter issues before the issues are loaded.");

        var filtered = new List<GitHubLabel>();

        if (filter.Names is not null)
        {
            var expl = _allLabels.Where(l => filter.Names.Contains(l.Name));
            filtered.AddRange(expl);
        }

        if (filter.Pattern is not null)
        {
            var pattern = new Regex(filter.Pattern);
            filtered.AddRange(_allLabels.Where(label => pattern.IsMatch(label.Name)));
        }

        return filtered;
    }
}
