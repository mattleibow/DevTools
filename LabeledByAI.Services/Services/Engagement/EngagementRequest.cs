namespace LabeledByAI.Services;

public record EngagementRequest(
    int Version,
    EngagementRequestIssue? Issue = null,
    EngagementRequestProject? Project = null);
