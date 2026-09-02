using BugCycle.Application.DTOs.Labels;
using BugCycle.Domain.Entities;

namespace BugCycle.Application.Mapping;

public static class LabelMappings
{
    public static LabelDto ToDto(this Label label, int usageCount = 0) => new()
    {
        Id = label.Id,
        Name = label.Name,
        ColorHex = label.ColorHex,
        UsageCount = usageCount
    };

    public static List<LabelDto> ToDtos(
        this IEnumerable<Label> labels,
        IReadOnlyDictionary<int, int>? usages = null)
        => labels
            .Select(l => l.ToDto(usages is not null && usages.TryGetValue(l.Id, out var c) ? c : 0))
            .ToList();
}
