using LabeledByAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LabeledByAI;

public class EngagementScoreFunction(EngagementService service, ILogger<EngagementScoreFunction> logger)
    : BaseFunction<EngagementRequest>(logger)
{
    [Function("engagement-score")]
    public override Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request) =>
        base.Run(request);

    protected override async Task<IActionResult> OnRun(HttpRequest request, EngagementRequest parsedBody)
    {
        var response = await service.CalculateScoresAsync(parsedBody, request.GetGithubToken());
        return new OkObjectResult(response);
    }
}
