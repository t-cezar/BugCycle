using BugCycle.Domain.Enums;

namespace BugCycle.Domain.Entities;

public class Sprint
{
    public int Id { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public SprintStatus Status { get; set; }

    public ICollection<Issue> Issues { get; set; } = new List<Issue>();
}
