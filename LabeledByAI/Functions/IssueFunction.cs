using LabeledByAI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LabeledByAI;

public class IssueFunction(IGitHubConnection githubConnection, ILogger<IssueFunction> logger)
    : BaseFunction<LabelSelectorRequestIssue>(logger)
{
    [Function("issue")]
    public override Task<IResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request) =>
        base.Run(request);

    protected override async Task<IResult> OnRun(HttpRequest request, LabelSelectorRequestIssue parsedBody)
    {
        githubConnection.SetToken(request.GetGithubToken());

        var github = new GitHub(githubConnection);
        var repo = github.GetRepository(parsedBody.Owner, parsedBody.Repo);
        var issue = await repo.GetIssueDetailedAsync(parsedBody.Number);

        return TypedResults.Ok(issue);
    }
}
