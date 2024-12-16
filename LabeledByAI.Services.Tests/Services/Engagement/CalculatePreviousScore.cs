using FluentAssertions;

namespace LabeledByAI.Services.Tests;

public partial class EngagementServiceUnitTests
{
    public class CalculatePreviousScore
    {
        EngagementService service = new(default!, default!);

        [Fact]
        public void BasicTestIssuePreviousScoreIsCorrect()
        {
            var issue = BasicExpectedIssues[0] with { TotalComments = 0, TotalReactions = 0 };

            var score = service.CalculatePreviousScore(issue);

            score.Should().Be(2);
        }
    }
}
