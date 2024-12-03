namespace LabeledByAI.Services;

public record LabelSelectorRequestLabels(
    string[]? Names,
    string? Pattern);
