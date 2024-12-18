using LabeledByAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LabeledByAI;

public class LabelSelectorFunction(GitHubRemoteConnection connection, LabelSelectorService service, ILogger<LabelSelectorFunction> logger)
    : BaseFunction<LabelSelectorRequest>(logger)
{
    [Function("label")]
    public override Task<IResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request) =>
        base.Run(request);

    protected override async Task<IResult> OnRun(HttpRequest request, LabelSelectorRequest parsedBody)
    {
        connection.SetToken(request.GetGithubToken());
        var response = await service.SelectLabelAsync(parsedBody);
        return TypedResults.Json(response, JsonExtensions.SerializerOptions);
    }
}
