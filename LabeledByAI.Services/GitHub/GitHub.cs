using Octokit.GraphQL;

namespace LabeledByAI.Services;

public class GitHub(Connection connection)
{
    private readonly Dictionary<(string Owner, string Repo), GitHubRepository> _repositories = [];
    private readonly Dictionary<(string Owner, int Number), GitHubProject> _projects = [];

    public GitHub(string githubToken, string applicationName = "labeled-by-ai")
        : this(new Connection(new ProductHeaderValue(applicationName), githubToken))
    {
    }

    public Connection Connection { get; } = connection;

    public GitHubRepository GetRepository(string owner, string repo)
    {
        if (!_repositories.TryGetValue((owner, repo), out var instance))
        {
            instance = new GitHubRepository(this, owner, repo);
            _repositories[(owner, repo)] = instance;
        }

        return instance;
    }

    public GitHubProject GetProject(string owner, int number)
    {
        if (!_projects.TryGetValue((owner, number), out var instance))
        {
            instance = new GitHubProject(this, owner, number);
            _projects[(owner, number)] = instance;
        }

        return instance;
    }
}
