using Octokit.GraphQL;
using System.Text.RegularExpressions;

namespace LabeledByAI.Services;

public class GitHubRepository(Connection connection, string owner, string repo)
{
    private List<GitHubLabel>? _allLabels;
    private readonly Dictionary<int, GitHubIssue> _allIssues = [];

    public async Task<IList<GitHubLabel>> GetLabelsAsync(GitHubLabelFilter? filter = null)
    {
        _allLabels ??= await FetchLabelsAsync();

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
            issue = await FetchIssueAsync(number);
            _allIssues[number] = issue;
        }

        return issue;
    }

    private async Task<List<GitHubLabel>> FetchLabelsAsync()
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Labels()
            .AllPages()
            .Select(label => new GitHubLabel(
                label.Id.ToString(),
                label.Name,
                label.Description
            ))
            .Compile();

        var labels = await connection.Run(query);

        return labels.ToList();
    }

    private async Task<GitHubIssue> FetchIssueAsync(int number)
    {
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issue(number: number)
            .Select(label => new GitHubIssue(
                label.Id.ToString(),
                label.Number,
                label.Title,
                label.Body
            ))
            .Compile();

        var issue = await connection.Run(query);

        return issue;
    }

    private IList<GitHubLabel> GetFilteredLabels(GitHubLabelFilter filter)
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
