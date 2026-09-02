using System.ComponentModel.DataAnnotations;

namespace BugCycle.Domain.Enums;

public enum IssuePriority
{
    [Display(Name = "Low")]
    Low = 0,

    [Display(Name = "Medium")]
    Medium = 1,

    [Display(Name = "High")]
    High = 2,

    [Display(Name = "Critical")]
    Critical = 3
}
