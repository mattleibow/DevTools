using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace LabeledByAI.Services;

public class EngagementService(ILogger<EngagementService> logger)
{
    public async Task<IList<EngagementResponse>> CalculateScoresAsync(EngagementRequest request, string githubToken)
    {
        // TODO:
        // - download the github issue and all the comments and reactions
        //   - issues touched in the last [7] days
        //   - download linked PRs
        //   - merge duplicated issues
        // OPTIONS:
        // - calc all issues
        // - calc only one issue
        // - calc all issues in the last [7/10/30] days
        // - calc all issues in a project
        // - calc all issues in a project in the last [7/10/30] days

        var reqIssue = request.Issue;

        // get github repository
        var github = new GitHub(githubToken);
        var repo = github.GetRepository(reqIssue.Owner, reqIssue.Repo);

        IList<GitHubIssue> issues;
        if (reqIssue.Number is int number)
        {
            // load the single issue
            logger.LogInformation("Loading the issue details...");

            var issue = await repo.GetIssueDetailedAsync(number);
            issues = [issue];
        }
        else
        {
            // load all the isses
            logger.LogInformation("Loading all the issue details...");

            issues = await repo.GetAllIssuesDetailedAsync();
        }

        if (!IsValidRequest(issues, out var errorResult))
            throw new InvalidOperationException(errorResult);

        var items = new List<EngagementResponse>(issues.Count);
        foreach (var issue in issues)
        {
            var score = CalculateScore(issue);

            var item = new EngagementResponse(
                new(issue.Number),
                new(score));

            items.Add(item);
        }

        return items;
    }

    private bool IsValidRequest(
        [NotNullWhen(true)] IList<GitHubIssue> issues,
        [NotNullWhen(false)] out string? errorResult)
    {
        if (issues is null || issues.Count == 0)
        {
            logger.LogError("Unable to load issues from GitHub.");
            errorResult = "The issues could not be loaded.";
            return false;
        }

        errorResult = null;
        return true;
    }

    private int CalculateScore(GitHubIssue issue)
    {
        // Components:
        //  - Number of Comments       => Indicates discussion and interest
        //  - Number of Reactions      => Shows emotional engagement
        //  - Number of Contributors   => Reflects the diversity of input
        //  - Time Since Last Activity => More recent activity indicates higher engagement
        //  - Issue Age                => Older issues might need more attention
        //  - Number of Linked PRs     => Shows active work on the issue
        var totalComments = issue.TotalUserComments;
        var totalReactions = issue.TotalReactions + issue.TotalCommentReactions;
        var contributors = issue.TotalUserContributors;
        var lastActivity = Math.Max(1, (int)issue.TimeSinceLastActivity.TotalDays);
        var issueAge = Math.Max(1, (int)issue.Age.TotalDays);
        var linkedPullRequests = 0;// issue.LinkedPullRequests.Count;

        // Weights:
        const int CommentsWeight = 3;
        const int ReactionsWeight = 1;
        const int ContributorsWeight = 2;
        const int LastActivityWeight = 1;
        const int IssueAgeWeight = 1;
        const int LinkedPullRequestsWeight = 2;

        return
            (CommentsWeight * totalComments) +
            (ReactionsWeight * totalReactions) +
            (ContributorsWeight * contributors) +
            (LastActivityWeight * (1 / lastActivity)) +
            (IssueAgeWeight * (1 / issueAge)) +
            (LinkedPullRequestsWeight * linkedPullRequests);
    }
}
