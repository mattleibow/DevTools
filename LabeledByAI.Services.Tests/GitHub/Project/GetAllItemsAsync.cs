using FluentAssertions;

namespace LabeledByAI.Services.Tests;

public partial class GitHubProjectUnitTests
{
    public class GetAllItemsAsync
    {
        [Fact]
        public async Task IncludeClosedIssuesIncludesClosedIssues()
        {
            var project = GetProject(BasicExpectedProjectItems, BasicExpectedIssues);

            var items = await project.GetAllItemsAsync(true);
            var issues = items.Select(i => i.Content).ToList();

            issues.Should()
                .BeEquivalentTo(BasicExpectedIssues, options => options
                    .Excluding(o => o.Age)
                    .Excluding(o => o.TimeSinceLastActivity));
        }

        [Fact]
        public async Task ExcludeClosedIssuesExcludesClosedIssues()
        {
            var project = GetProject(BasicExpectedProjectItems, BasicExpectedIssues);

            var items = await project.GetAllItemsAsync(false);
            var issues = items.Select(i => i.Content).ToList();

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
