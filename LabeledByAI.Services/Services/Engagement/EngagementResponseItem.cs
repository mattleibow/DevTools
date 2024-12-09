namespace LabeledByAI.Services;

public record EngagementResponseItem(
    string? Id,
    EngagementResponseIssue Issue,
    EngagementResponseEngagement Engagement);
