using Microsoft.Extensions.Logging;
using System.Net;

namespace LabeledByAI.Services;

public class EngagementService(IGitHubConnection githubConnection, ILogger<EngagementService> logger)
{
    public async Task<EngagementResponse> CalculateScoresAsync(EngagementRequest request, string githubToken)
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

        githubConnection.SetToken(githubToken);
        var github = new GitHub(githubConnection);

        if (request.Issue is { } reqIssue)
        {
            return await CalculateScoreAsync(reqIssue, github);
        }

        if (request.Project is { } reqProject)
        {
            return await CalculateScoreAsync(reqProject, github);
        }

        logger.LogError("Request had neither an issue or project.");
        throw new ArgumentException("Request had neither an issue or project.", nameof(request));
    }

    private async Task<EngagementResponse> CalculateScoreAsync(EngagementRequestIssue reqIssue, GitHub github)
    {
        IReadOnlyList<GitHubIssue> issues;
        try
        {
            // get github repository
            var repo = github.GetRepository(reqIssue.Owner, reqIssue.Repo);

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
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException($"Unable to read any issues from the GitHub repository. {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to read any issues from the GitHub repository. {ex.Message}", ex);
        }

        if (issues is null || issues.Count == 0)
        {
            logger.LogWarning("No matching issues were found on GitHub, returing an empty response.");
            return new EngagementResponse([], 0);
        }

        // calculate the engagement score for each issue
        var items = new List<EngagementResponseItem>(issues.Count);
        foreach (var issue in issues)
        {
            var score = CalculateScore(issue);

            var item = new EngagementResponseItem(
                null,
                new EngagementResponseIssue(
                    issue.Id,
                    reqIssue.Owner,
                    reqIssue.Repo,
                    issue.Number),
                new EngagementResponseEngagement(
                    score));

            items.Add(item);
        }

        return new EngagementResponse(
            items,
            items.Count);
    }

    private async Task<EngagementResponse> CalculateScoreAsync(EngagementRequestProject reqProject, GitHub github)
    {
        string projectId;
        IReadOnlyList<GitHubProjectItem> projectItems;
        try
        {
            // get github project
            var project = github.GetProject(reqProject.Owner, reqProject.Number);

            // load all the items
            logger.LogInformation("Loading all the project details...");

            projectId = await project.GetProjectIdAsync();
            projectItems = await project.GetAllItemsDetailedAsync();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException($"Unable to read any issues from the GitHub repository. {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Unable to read any issues from the GitHub repository. {ex.Message}", ex);
        }

        if (projectItems is null || projectItems.Count == 0)
        {
            logger.LogWarning("No matching items were found on GitHub, returing an empty response.");
            return new EngagementResponse([], 0);
        }

        // calculate the engagement score for each item
        var items = new List<EngagementResponseItem>(projectItems.Count);
        foreach (var projectItem in projectItems)
        {
            var (content, score) = projectItem.Content switch
            {
                GitHubIssue issue => (
                    new EngagementResponseIssue(
                        issue.Id,
                        issue.Owner,
                        issue.Repository,
                        issue.Number),
                    CalculateScore(issue)),
                _ => (null, 0)
            };

            if (content is null)
                continue;

            var item = new EngagementResponseItem(
                projectItem.Id,
                content,
                new EngagementResponseEngagement(
                    score));

            items.Add(item);
        }

        return new EngagementResponse(
            items,
            items.Count,
            new EngagementResponseProject(
                projectId,
                reqProject.Owner,
                reqProject.Number));
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
