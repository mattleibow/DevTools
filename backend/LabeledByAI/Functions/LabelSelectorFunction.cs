using LabeledByAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LabeledByAI;

public class LabelSelectorFunction(LabelSelectorService service, ILogger<LabelSelectorFunction> logger)
    : BaseFunction<LabelSelectorRequest>(logger)
{
    [Function("label")]
    public override Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request) =>
        base.Run(request);

    protected override async Task<IActionResult> OnRun(HttpRequest request, LabelSelectorRequest parsedBody)
    {
        var response = await service.SelectLabelAsync(parsedBody, request.GetGithubToken());
        return new OkObjectResult(response);
    }
}
