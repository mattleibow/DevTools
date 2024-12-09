using Microsoft.AspNetCore.Http;

namespace LabeledByAI;

public static class HttpRequestExtensions
{
    private const string GitHubTokenHeaderName = "X-GitHub-Token";

    public static string GetGithubToken(this HttpRequest request)
    {
        var githubToken = request.Headers[GitHubTokenHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(githubToken))
        {
            throw new ProblemDetailsException(new()
            {
                Status = StatusCodes.Status401Unauthorized,
                Detail = "No GitHub token was provided in the request headers."
            });
        }

        return githubToken;
    }
}
