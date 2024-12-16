using FluentAssertions;

namespace LabeledByAI.Services.Tests;

public partial class EngagementServiceUnitTests
{
    public class CalculateScore
    {
        EngagementService service = new(default!, default!);

        [Fact]
        public void BrandNewIssue()
        {
            var issue = Helpers.CreateIssue();
            service.CalculateScore(issue).Should().Be(4);
        }

        [Fact]
        public void AncientIssue()
        {
            var issue = Helpers.CreateIssue(
                createdOn: DateTimeOffset.Now.AddYears(-1),
                lastActivityOn: DateTimeOffset.Now.AddYears(-1));

            service.CalculateScore(issue).Should().Be(2);
        }

        [Fact]
        public void BrandNewIssueIsHigherThanAncientIssue()
        {
            var ancient = Helpers.CreateIssue(
                createdOn: DateTimeOffset.Now.AddYears(-1),
                lastActivityOn: DateTimeOffset.Now.AddYears(-1));

            var brandNew = Helpers.CreateIssue();

            var brandNewScore = service.CalculateScore(brandNew);
            var ancientScore = service.CalculateScore(ancient);

            brandNewScore.Should().BeGreaterThan(ancientScore);
        }

        [Fact]
        public void BasicTestIssueScoreIsCorrect()
        {
            var issue = BasicExpectedIssues[0];
            var details = BasicExpectedDetails[issue.Number];
            issue.Comments = details.Comments;
            issue.Reactions = details.Reactions;

            var score = service.CalculateScore(issue);

            score.Should().Be(11);
        } 
    }
}
