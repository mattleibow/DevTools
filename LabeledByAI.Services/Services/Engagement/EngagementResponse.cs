namespace LabeledByAI.Services;

public record EngagementResponse(
    IList<EngagementResponseItem> Items,
    int TotalItems,
    EngagementResponseProject? Project = null);
