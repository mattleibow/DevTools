namespace LabeledByAI.Services;

public interface IGitHubConnection
{
    void SetToken(string githubToken);

    Task<IReadOnlyList<GitHubLabel>> FetchLabelsAsync(string owner, string repo);

    Task<GitHubIssue> FetchIssueAsync(string owner, string repo, int number);

    Task<IReadOnlyList<GitHubIssue>> FetchAllIssuesAsync(string owner, string repo, bool includeClosed = false);

    Task<IReadOnlyList<GitHubComment>> FetchIssueCommentsAsync(string owner, string repo, int number);

    Task<string> FetchProjectIdAsync(string owner, int number);

    Task<IReadOnlyList<GitHubProjectItem>> FetchAllProjectItemsAsync(string projectId);
}
