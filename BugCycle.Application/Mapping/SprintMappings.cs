using BugCycle.Application.DTOs.Sprints;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.Mapping;

public static class SprintMappings
{
    public static SprintDto ToDto(this Sprint sprint, IReadOnlyCollection<Issue>? issues = null)
    {
        var source = issues ?? sprint.Issues?.ToList() ?? new List<Issue>();

        return new SprintDto
        {
            Id = sprint.Id,
            ProjectId = sprint.ProjectId,
            Name = sprint.Name,
            Goal = sprint.Goal,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Status = sprint.Status,
            IssueCount = source.Count,
            DoneCount = source.Count(i => i.Status == IssueStatus.Done),
            TotalStoryPoints = source.Sum(i => i.StoryPoints ?? 0),
            DoneStoryPoints = source
                .Where(i => i.Status == IssueStatus.Done)
                .Sum(i => i.StoryPoints ?? 0)
        };
    }
}
