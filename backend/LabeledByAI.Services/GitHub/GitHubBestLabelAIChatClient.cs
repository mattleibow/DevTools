using Microsoft.Extensions.Logging;
using System.Text;

namespace LabeledByAI.Services;

public class GitHubBestLabelAIChatClient(AIChatClient chatClient, ILogger<GitHubBestLabelAIChatClient> logger)
{
    public async Task<string> GetBestLabelAsync(GitHubIssue issue, IList<GitHubLabel> labels)
    {
        logger.LogInformation("Generating OpenAI request...");

        var systemPrompt = GetSystemPrompt(labels);
        var assistantPrompt = GetIssuePrompt(issue);

        var response = await chatClient.QueryAIAsync(systemPrompt, assistantPrompt);

        return response;
    }

    private static string GetSystemPrompt(params IEnumerable<GitHubLabel> labels) =>
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

        If no labels match or can be assigned, then you are to
        reply with a null label and null reason. 
        The only labels that are valid for assignment are found
        between the "===== Available Labels =====" lines. Do not
        return a label if that label is not found in there.

        Some labels have an additional description that should be
        used in order to find the best match.

        You are to also provide a reason as to why that label was 
        selected to make sure that everyone knows why. Also, you
        need to make sure to mention other related labels and why
        they were not a good selection for the issue. Give a reason
        in 50 to 100 words.

        ===== Available Labels =====
        {{GetPromptLabelList(labels)}}
        ===== Available Labels =====
        
        Please reply in json with the format and only in this format:

        { 
            "label": "LABEL_NAME_HERE",
            "reason": "REASON_FOR_LABEL_HERE"
        }

        """;

    private static string GetPromptLabelList(IEnumerable<GitHubLabel> labels)
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

    private static string GetIssuePrompt(GitHubIssue issue) => $"""
        A new issue has arrived, please label it correctly and accurately.
        
        The issue title is:
        {issue.Title ?? "-"}
        
        The issue body is:
        {issue.Body}
        """;
}
