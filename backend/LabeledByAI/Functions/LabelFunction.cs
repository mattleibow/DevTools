using LabeledByAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Octokit.GraphQL;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace LabeledByAI;

public class LabelFunction(GitHubBestLabelAIChatClient chatClient, ILogger<LabelFunction> logger)
{
    [Function("label")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
    {
        logger.LogInformation("Starting to process a new issue...");

        var labelRequest = await ParseLabelRequestAsync(request);
        if (!ValidateRequest(labelRequest, out var errorResult))
        {
            return errorResult;
        }

        var (issue, labels) = await FetchGitHubObjects(request, labelRequest);
        if (!ValidateRequestIssue(issue, labels, out errorResult))
        {
            return errorResult;
        }

        logger.LogInformation("The new issue is a valid object.");

        var responseJson = await chatClient.GetBestLabelAsync(issue, labels);

        return new OkObjectResult(responseJson);
    }

    private async Task<LabelRequest?> ParseLabelRequestAsync(HttpRequest request)
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<LabelRequest>(request.Body, SerializerOptions);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deserialize the request body.");
            return null;
        }
    }

    private bool ValidateRequest([NotNullWhen(true)] LabelRequest? labelRequest, [NotNullWhen(false)] out IActionResult? errorResult)
    {
        if (labelRequest is null)
        {
            logger.LogError("No new issue was provided in the request body.");
            errorResult = new BadRequestObjectResult("The new issue is null.");
            return false;
        }

        errorResult = null;
        return true;
    }

    private async Task<(GitHubIssue? Issue, IList<GitHubLabel>? Labels)> FetchGitHubObjects(HttpRequest request, LabelRequest labelRequest)
    {
        var githubToken = request.Headers["X-GitHub-Token"].ToString();
        if (string.IsNullOrWhiteSpace(githubToken))
        {
            logger.LogError("No GitHub token was provided in the request headers.");
            return (null, null);
        }

        var githubConnection = new Connection(
            new ProductHeaderValue("labeled-by-ai"), githubToken);

        var github = new GitHubRepository(
            githubConnection,
            labelRequest.Issue.Owner,
            labelRequest.Issue.Repo);

        // load all labels from the repository
        logger.LogInformation("Loading all labels from the repo...");
        var labels = await github.GetLabelsAsync(new(labelRequest.Labels.Names, labelRequest.Labels.Pattern));

        // load issue details from the repository
        logger.LogInformation("Loading the issue details...");
        var issue = await github.GetIssueAsync(labelRequest.Issue.Number);

        return (issue, labels);
    }

    private bool ValidateRequestIssue([NotNullWhen(true)] GitHubIssue? issue, [NotNullWhen(true)] IList<GitHubLabel>? labels, [NotNullWhen(false)] out IActionResult? errorResult)
    {
        if (string.IsNullOrWhiteSpace(issue?.Id))
        {
            logger.LogError("Unable to load issue details from GitHub.");
            errorResult = new BadRequestObjectResult("The issue could not be loaded.");
            return false;
        }

        if (labels is null || labels.Count == 0)
        {
            logger.LogError("Unable to load labels from GitHub.");
            errorResult = new BadRequestObjectResult("The labels could not be loaded.");
            return false;
        }

        errorResult = null;
        return true;
    }

    public record LabelRequest(
        int Version,
        LabelRequestIssue Issue,
        LabelRequestLabels Labels);

    public record LabelRequestLabels(
        string[]? Names,
        string? Pattern);

    public record LabelRequestIssue(
        string Owner,
        string Repo,
        int Number);

    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
}
