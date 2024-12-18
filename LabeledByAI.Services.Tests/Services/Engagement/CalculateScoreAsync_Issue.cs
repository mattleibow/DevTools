using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LabeledByAI.Services.Tests;

public partial class EngagementServiceUnitTests
{
    public class CalculateScoreAsync_Issue
    {
        private readonly IGitHubConnection githubConnection;
        private readonly EngagementService service;
        private readonly GitHub github;

        public CalculateScoreAsync_Issue()
        {
            githubConnection = GetConnection(
                projectItems: BasicExpectedProjectItems,
                issues: BasicExpectedIssues,
                details: BasicExpectedDetails);

            service = new(
                githubConnection,
                Substitute.For<ILogger<EngagementService>>());

            github = new GitHub(githubConnection);
        }

        [Fact]
        public async Task OldIssueHasCorrectScores()
        {
            var expectedIssue = BasicExpectedIssues[0];
            var expectedResponseIssue = new EngagementResponseIssue(
                expectedIssue.Id,
                expectedIssue.Owner,
                expectedIssue.Repository,
                expectedIssue.Number);
            var expectedResponseEngagement = new EngagementResponseEngagement(
                Score: 11, 
                PreviousScore: 5,
                EngagementResponseEngagementClassification.Hot);

            var response = await service.CalculateScoreAsync(
                new EngagementRequestIssue(TestOwner, TestRepo, 1),
                github);

            response.Project.Should().BeNull();
            response.TotalItems.Should().Be(1);
            var item = response.Items.Should().ContainSingle().Subject;

            item.Id.Should().BeNull();
            item.Issue.Should().BeEquivalentTo(expectedResponseIssue);
            item.Engagement.Should().BeEquivalentTo(expectedResponseEngagement);
        }
    }
}
