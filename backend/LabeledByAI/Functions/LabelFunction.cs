using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Octokit.GraphQL;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LabeledByAI;

public class LabelFunction(IChatClient chatClient, ILogger<LabelFunction> logger)
{
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

    public record RepoLabel(
        string Id,
        string Name,
        string Description);

    public record RepoIssue(
        string Id,
        int Number,
        string Title,
        string Body);

    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

    [Function("label")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
    {
        logger.LogInformation("Starting to process a new issue...");

        var newIssue = await GetNewRequest(request);

        if (!ValidateRequest(newIssue, out var errorResult))
        {
            return errorResult;
        }

        logger.LogInformation("The new issue is a valid object.");

        // Initialize GitHub client with the personal access token
        var githubToken = request.Headers["X-GitHub-Token"].ToString();
        var githubConnection = new Connection(
            new ProductHeaderValue("labeled-by-ai"), githubToken);

        // load all labels from the repository
        logger.LogInformation("Loading all labels from the repo...");
        var labels = await GetLabelsAsync(newIssue.Issue, githubConnection);
        var filteredLabels = FilterLabels(labels, newIssue.Labels);

        // load issue details from the repository
        logger.LogInformation("Loading the issue details...");
        var issue = await GetIssueAsync(newIssue.Issue, githubConnection);

        logger.LogInformation("Generating OpenAI request...");

        IList<ChatMessage> messages =
        [
            new(ChatRole.System, GetSystemPrompt(filteredLabels)),
            new(ChatRole.Assistant, GetIssuePrompt(issue.Title, issue.Body)),
        ];

        logger.LogInformation(
            $"""
            messages >>>
            {string.Join(Environment.NewLine, messages.Select(m => $"{m.Role} => {m.Text}"))}
            <<< messages
            """);

        logger.LogInformation("Sending a request to OpenAI...");

        var options = new ChatOptions
        {
            MaxOutputTokens = 400,
        };
        var response = await chatClient.CompleteAsync(messages, options);

        logger.LogInformation("OpenAI has replied.");

        logger.LogInformation(
            $"""
            response >>>
            {response}
            <<< response
            """);

        return new OkObjectResult(response.ToString());
    }

    private async Task<RepoIssue> GetIssueAsync(LabelRequestIssue issueInfo, Connection connection)
    {
        // Define the query to get the issue from the repository
        var owner = issueInfo.Owner;
        var repo = issueInfo.Repo;
        var number = issueInfo.Number;
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Issue(number: number)
            .Select(label => new RepoIssue(
                label.Id.ToString(),
                label.Number,
                label.Title,
                label.Body
            ))
            .Compile();

        // Execute the query
        var issue = await connection.Run(query);

        // Return the issue
        return issue;
    }

    private IList<RepoLabel> FilterLabels(IList<RepoLabel> labels, LabelRequestLabels labelInfo)
    {
        var filtered = new List<RepoLabel>();

        if (labelInfo.Names is not null)
        {
            var expl = labels.Where(l => labelInfo.Names.Contains(l.Name));
            filtered.AddRange(expl);
        }

        if (labelInfo.Pattern is not null)
        {
            var pattern = new Regex(labelInfo.Pattern);
            filtered.AddRange(labels.Where(label => pattern.IsMatch(label.Name)));
        }

        return filtered;
    }

    private async Task<IList<RepoLabel>> GetLabelsAsync(LabelRequestIssue issueInfo, Connection connection)
    {
        // Define the query to get all labels in a repository
        var owner = issueInfo.Owner;
        var repo = issueInfo.Repo;
        var query = new Query()
            .Repository(owner: owner, name: repo)
            .Labels()
            .AllPages()
            .Select(label => new RepoLabel(
                label.Id.ToString(),
                label.Name,
                label.Description
            ))
            .Compile();

        // Execute the query
        var labels = await connection.Run(query);

        // Return the labels
        return labels.ToList();
    }

    private bool ValidateRequest([NotNullWhen(true)] LabelRequest? newIssue, [NotNullWhen(false)] out IActionResult? errorResult)
    {
        if (newIssue is null)
        {
            logger.LogError("No new issue was provided in the request body.");
            errorResult = new BadRequestObjectResult("The new issue is null.");
            return false;
        }

        //if (string.IsNullOrWhiteSpace(newIssue.))
        //{
        //    logger.LogError("No new issue body was provided in the request body.");
        //    return new BadRequestObjectResult("The new issue body is null.");
        //}

        //if (newIssue.Labels is null || newIssue.Labels.Length == 0)
        //{
        //    logger.LogError("No labels wer provided in the request body.");
        //    return new BadRequestObjectResult("No labels provided.");
        //}

        errorResult = null;
        return true;
    }

    private async Task<LabelRequest?> GetNewRequest(HttpRequest request)
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

    private static string GetSystemPrompt(params IEnumerable<RepoLabel> labels) =>
        $$"""
        You are an expert developer who is able to correctly and
        accurately assign labels to new issues that are opened.

        You are to pick from the following list of labels and
        assign just one of them. If none of the labels are
        correct, do not assign any labels. If no issue content 
        was provided or if there is not enough content to make
        a decision, do not assign any labels. If the label that
        you have selected is not in the list of labels, then
        do not assign any labels.

        You also provide a reason as to why that label was 
        selected to make sure that everyone knows why. Also,
        mention other related labels and why they were not a
        good selection.

        If no labels match or can be assigned, then you are to
        reply with a null label and null reason. 
        The only labels that are valid for assignment are found
        between the "===== Available Labels =====" lines. Do not
        return a label if that label is not found in there.

        Some labels have an additional description that should be
        used in order to find the best match.

        ===== Available Labels =====
        {{GetPromptLabelList(labels)}}
        ===== Available Labels =====
        
        Please reply in json with the format and only in this format:

        { 
            "label": "LABEL_NAME_HERE",
            "reason": "REASON_FOR_LABEL_HERE"
        }

        """;

    private static string GetPromptLabelList(IEnumerable<RepoLabel> labels)
    {
        var sb = new StringBuilder();

        foreach (var label in labels)
        {
            sb.AppendLine($"- name: {label.Name}");
            if (!string.IsNullOrWhiteSpace(label.Description))
            {
                sb.AppendLine($"  description: {label.Description}");
            }
        }

        return sb.ToString();
    }

    private static string GetIssuePrompt(string? title, string body) => $"""
        A new issue has arrived, please label it correctly and accurately.
        
        The issue title is:
        {title ?? "-"}
        
        The issue body is:
        {body}
        """;
}
