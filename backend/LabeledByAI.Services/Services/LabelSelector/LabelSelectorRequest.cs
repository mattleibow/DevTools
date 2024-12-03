namespace LabeledByAI.Services;

public record LabelSelectorRequest(
    int Version,
    LabelSelectorRequestIssue Issue,
    LabelSelectorRequestLabels Labels);
