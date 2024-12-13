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
            var repo = GetRepo(BasicExpectedIssues, BasicExpectedDetails);
            var connection = repo.GitHub.Connection;

            _ = await repo.GetAllIssuesDetailedAsync(true);

            await connection.Received().FetchAllIssuesAsync(TestOwner, TestRepo, true);
            await connection.Received(3).FetchIssueDetailsAsync(TestOwner, TestRepo, Arg.Any<int>());
            await connection.DidNotReceive().FetchIssueCommentsAsync(TestOwner, TestRepo, Arg.Any<int>());
        }

        [Fact]
        public async Task IncludeClosedIssuesIncludesClosedIssues()
        {
            var repo = GetRepo(BasicExpectedIssues, BasicExpectedDetails);

            var issues = await repo.GetAllIssuesDetailedAsync(true);

            issues[0].Comments.Should()
                .BeEquivalentTo(BasicExpectedDetails[1].Comments, options => options
                    .Excluding(o => o.Age));
            issues[1].Comments.Should()
                .BeEquivalentTo(BasicExpectedDetails[2].Comments, options => options
                    .Excluding(o => o.Age));
            issues[2].Comments.Should().BeNullOrEmpty();

            issues[0].Reactions.Should().BeEquivalentTo(BasicExpectedDetails[1].Reactions);
            issues[1].Reactions.Should().BeNullOrEmpty();
            issues[2].Reactions.Should().BeEquivalentTo(BasicExpectedDetails[3].Reactions);
        }

        [Fact]
        public async Task ExcludeClosedIssuesExcludesClosedIssues()
        {
            var repo = GetRepo(BasicExpectedIssues, BasicExpectedDetails);

            var issues = await repo.GetAllIssuesDetailedAsync(false);

            issues[0].Comments.Should()
                .BeEquivalentTo(BasicExpectedDetails[1].Comments, options => options
                    .Excluding(o => o.Age));
            issues[1].Comments.Should().BeNullOrEmpty();

            issues[0].Reactions.Should().BeEquivalentTo(BasicExpectedDetails[1].Reactions);
            issues[1].Reactions.Should().BeEquivalentTo(BasicExpectedDetails[3].Reactions);
        }
    }
}
