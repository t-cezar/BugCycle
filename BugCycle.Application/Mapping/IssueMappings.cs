using BugCycle.Application.DTOs.Issues;
using BugCycle.Domain.Entities;

namespace BugCycle.Application.Mapping;

public static class IssueMappings
{
    public static IssueDto ToDto(this Issue issue)
    {
        var projectKey = issue.Project?.Key ?? string.Empty;

        return new IssueDto
        {
            Id = issue.Id,
            Number = issue.Number,
            DisplayKey = $"{projectKey}-{issue.Number}",
            ProjectId = issue.ProjectId,
            ProjectKey = projectKey,
            ProjectName = issue.Project?.Name ?? string.Empty,
            SprintId = issue.SprintId,
            SprintName = issue.Sprint?.Name,
            Title = issue.Title,
            Type = issue.Type,
            Status = issue.Status,
            Priority = issue.Priority,
            Reporter = issue.Reporter?.ToSummaryDto() ?? new(),
            Assignee = issue.Assignee?.ToSummaryDto(),
            StoryPoints = issue.StoryPoints,
            BoardOrder = issue.BoardOrder,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            ClosedAt = issue.ClosedAt,
            Labels = issue.IssueLabels?
                .Where(il => il.Label is not null)
                .Select(il => il.Label.ToDto())
                .OrderBy(l => l.Name)
                .ToList() ?? new()
        };
    }

    public static List<IssueDto> ToDtos(this IEnumerable<Issue> issues)
        => issues.Select(i => i.ToDto()).ToList();

    public static IssueDetailDto ToDetailDto(
        this Issue issue,
        string? currentUserId,
        bool canEdit,
        bool canDelete,
        bool canComment,
        bool currentUserIsProjectOwner)
    {
        var basic = issue.ToDto();

        return new IssueDetailDto
        {
            Id = basic.Id,
            Number = basic.Number,
            DisplayKey = basic.DisplayKey,
            ProjectId = basic.ProjectId,
            ProjectKey = basic.ProjectKey,
            ProjectName = basic.ProjectName,
            SprintId = basic.SprintId,
            SprintName = basic.SprintName,
            Title = basic.Title,
            Type = basic.Type,
            Status = basic.Status,
            Priority = basic.Priority,
            Reporter = basic.Reporter,
            Assignee = basic.Assignee,
            StoryPoints = basic.StoryPoints,
            BoardOrder = basic.BoardOrder,
            CreatedAt = basic.CreatedAt,
            UpdatedAt = basic.UpdatedAt,
            ClosedAt = basic.ClosedAt,
            Labels = basic.Labels,
            Description = issue.Description,
            Comments = (issue.Comments ?? new List<Comment>())
                .ToDtos(currentUserId, currentUserIsProjectOwner),
            CanEdit = canEdit,
            CanDelete = canDelete,
            CanComment = canComment
        };
    }
}
