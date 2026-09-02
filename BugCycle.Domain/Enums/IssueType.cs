using System.ComponentModel.DataAnnotations;

namespace BugCycle.Domain.Enums;

public enum IssueType
{
    [Display(Name = "Bug")]
    Bug = 0,

    [Display(Name = "Task")]
    Task = 1,

    [Display(Name = "Story")]
    Story = 2
}
