using LabeledByAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LabeledByAI;

public class EngagementScoreFunction(GitHubRemoteConnection connection, EngagementService service, ILogger<EngagementScoreFunction> logger)
    : BaseFunction<EngagementRequest>(logger)
{
    [Function("engagement-score")]
    public override Task<IResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request) =>
        base.Run(request);

    protected override async Task<IResult> OnRun(HttpRequest request, EngagementRequest parsedBody)
    {
        connection.SetToken(request.GetGithubToken());
        var response = await service.CalculateScoresAsync(parsedBody);
        return TypedResults.Json(response, JsonExtensions.SerializerOptions);
    }
}
