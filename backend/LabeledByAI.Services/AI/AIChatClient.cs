using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LabeledByAI.Services;

public class AIChatClient(IChatClient chatClient, ILogger<AIChatClient> logger)
{
    public async Task<string> QueryAIAsync(string systemPrompt, string assistantPrompt)
    {
        logger.LogInformation("Generating OpenAI request...");

        IList<ChatMessage> messages =
        [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.Assistant, assistantPrompt),
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
            MaxOutputTokens = 1000,
        };
        var response = await chatClient.CompleteAsync(messages, options);

        logger.LogInformation("OpenAI has replied.");

        logger.LogInformation(
            $"""
            response >>>
            {response}
            <<< response
            """);

        var responseJson = response.ToString();

        return responseJson;
    }
}
