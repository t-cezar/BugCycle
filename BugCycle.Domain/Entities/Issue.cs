using BugCycle.Domain.Enums;

namespace BugCycle.Domain.Entities;

public class Issue
{
    public int Id { get; set; }

    public int Number { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? SprintId { get; set; }
    public Sprint? Sprint { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public IssueType Type { get; set; }
    public IssueStatus Status { get; set; }
    public IssuePriority Priority { get; set; }

    public string ReporterId { get; set; } = string.Empty;
    public ApplicationUser Reporter { get; set; } = null!;

    public string? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }

    public int? StoryPoints { get; set; }

    public int BoardOrder { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<IssueLabel> IssueLabels { get; set; } = new List<IssueLabel>();

    public string DisplayKey => $"{Project?.Key ?? "?"}-{Number}";
}
