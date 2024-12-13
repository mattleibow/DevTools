using FluentAssertions;

namespace LabeledByAI.Services.Tests;
public partial class GitHubIssueUnitTests
{
    public class TryGetHistoricIssue
    {
        [Fact]
        public void GettingHistoricIssueBeforeCreationIsInvalid()
        {
            var issue = Helpers.CreateIssue();

            var pastDate = issue.CreatedOn.AddDays(-1);

            var result = issue.TryGetHistoricIssue(pastDate, out var historic);

            result.Should().BeFalse();
            historic.Should().BeNull();
        }

        [Fact]
        public void GettingHistoricIssueAfterCreationIsValid()
        {
            var createDate = DateTimeOffset.Now.AddDays(-2);
            var pastDate = createDate.AddDays(1);

            var issue = Helpers.CreateIssue(
                createdOn: createDate,
                lastActivityOn: createDate);

            var expectedHistoric = Helpers.CreateIssue(
                createdOn: createDate,
                lastActivityOn: pastDate);

            var result = issue.TryGetHistoricIssue(pastDate, out var historic);

            result.Should().BeTrue();
            historic.Should().BeEquivalentTo(expectedHistoric, opt => opt
                .Excluding(o => o.Age)
                .Excluding(o => o.TimeSinceLastActivity));
        }
    }
}
