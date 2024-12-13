using FluentAssertions;
using NSubstitute;

namespace LabeledByAI.Services.Tests;

public partial class GitHubRepositoryUnitTests
{
    public class GetAllIssuesAsync
    {
        [Fact]
        public async Task SubstitutionWorks()
        {
            var repo = GetRepo(BasicExpectedIssues);
            var connection = repo.GitHub.Connection;

            _ = await repo.GetAllIssuesAsync();

            await connection.Received().FetchAllIssuesAsync(TestOwner, TestRepo);
            await connection.DidNotReceive().FetchIssueCommentsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>());
        }

        [Fact]
        public async Task IncludeClosedIssuesIncludesClosedIssues()
        {
            var repo = GetRepo(BasicExpectedIssues);

            var issues = await repo.GetAllIssuesAsync(true);

            issues.Should()
                .BeEquivalentTo(BasicExpectedIssues, options => options
                    .Excluding(o => o.Age)
                    .Excluding(o => o.TimeSinceLastActivity));
        }

        [Fact]
        public async Task ExcludeClosedIssuesExcludesClosedIssues()
        {
            var repo = GetRepo(BasicExpectedIssues);

            var issues = await repo.GetAllIssuesAsync(false);

            var expectedIssues = new[] {
                BasicExpectedIssues[0],
                BasicExpectedIssues[2]
            };

            issues.Should()
                .BeEquivalentTo(expectedIssues, options => options
                    .Excluding(o => o.Age)
                    .Excluding(o => o.TimeSinceLastActivity));
        }
    }
}
