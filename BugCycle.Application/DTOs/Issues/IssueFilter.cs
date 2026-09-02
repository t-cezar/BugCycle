using BugCycle.Domain.Enums;

namespace BugCycle.Application.DTOs.Issues;

public class IssueFilter
{
    public int? ProjectId { get; set; }

    public int? SprintId { get; set; }

    
    public bool BacklogOnly { get; set; }

    public string? AssigneeId { get; set; }

    public string? ReporterId { get; set; }

    public IssueType? Type { get; set; }

    public IssueStatus? Status { get; set; }

    public IssuePriority? Priority { get; set; }

    public int? LabelId { get; set; }

    public string? Search { get; set; }
}
