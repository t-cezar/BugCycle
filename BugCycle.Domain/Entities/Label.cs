namespace BugCycle.Domain.Entities;

public class Label
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ColorHex { get; set; } = "#6b7280";

    public ICollection<IssueLabel> IssueLabels { get; set; } = new List<IssueLabel>();
}
