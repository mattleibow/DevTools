using FluentAssertions;
using NSubstitute;

namespace LabeledByAI.Services.Tests;

public partial class GitHubRepositoryUnitTests
{
    public class GetAllIssuesDetailedAsync
    {
        [Fact]
        public async Task SubstitutionWorks()
        {
            var repo = GetRepo(BasicExpectedIssues, BasicExpectedComments);
            var connection = repo.GitHub.Connection;

            _ = await repo.GetAllIssuesDetailedAsync(true);

            await connection.Received().FetchAllIssuesAsync(TestOwner, TestRepo, true);
            await connection.Received(2).FetchIssueCommentsAsync(TestOwner, TestRepo, Arg.Is<int>(n => n == 1 || n == 2));
            await connection.DidNotReceive().FetchIssueCommentsAsync(TestOwner, TestRepo, 3);
        }

        [Fact]
        public async Task IncludeClosedIssuesIncludesClosedIssues()
        {
            var repo = GetRepo(BasicExpectedIssues, BasicExpectedComments);

            var issues = await repo.GetAllIssuesDetailedAsync(true);

            issues[0].Comments.Should()
                .BeEquivalentTo(BasicExpectedComments[1], options => options
                    .Excluding(o => o.Age));
            issues[1].Comments.Should()
                .BeEquivalentTo(BasicExpectedComments[2], options => options
                    .Excluding(o => o.Age));
            issues[2].Comments.Should().BeNullOrEmpty();
        }

        [Fact]
        public async Task ExcludeClosedIssuesExcludesClosedIssues()
        {
            var repo = GetRepo(BasicExpectedIssues, BasicExpectedComments);

            var issues = await repo.GetAllIssuesDetailedAsync(false);

            issues[0].Comments.Should()
                .BeEquivalentTo(BasicExpectedComments[1], options => options
                    .Excluding(o => o.Age));
            issues[1].Comments.Should().BeNullOrEmpty();
        }
    }
}
