using System.ComponentModel.DataAnnotations;
using BugCycle.Application.DTOs.Comments;
using BugCycle.Application.DTOs.Labels;
using BugCycle.Application.DTOs.Users;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.DTOs.Issues;

public class IssueDto
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string DisplayKey { get; set; } = string.Empty;

    public int ProjectId { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;

    public int? SprintId { get; set; }
    public string? SprintName { get; set; }

    public string Title { get; set; } = string.Empty;

    public IssueType Type { get; set; }
    public IssueStatus Status { get; set; }
    public IssuePriority Priority { get; set; }

    public UserSummaryDto Reporter { get; set; } = new();
    public UserSummaryDto? Assignee { get; set; }

    public int? StoryPoints { get; set; }
    public int BoardOrder { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public List<LabelDto> Labels { get; set; } = new();

    //True cand issue-ul nu este atasat niciunui sprint
    public bool IsInBacklog => SprintId is null;
}

public class IssueDetailDto : IssueDto
{
    public string? Description { get; set; }
    public List<CommentDto> Comments { get; set; } = new();

    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanComment { get; set; }
}
