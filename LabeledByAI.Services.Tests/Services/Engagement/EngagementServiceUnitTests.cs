namespace LabeledByAI.Services.Tests;

public partial class EngagementServiceUnitTests : GitHubUnitTests
{
    public class Weights
    {
        public const int Comments = 3;
        public const int Reactions = 1;
        public const int Contributors = 2;
        public const int LastActivity = 1;
        public const int IssueAge = 1;
        public const int LinkedPullRequests = 2;
    }
}
