using System.ComponentModel.DataAnnotations;

namespace BugCycle.Domain.Enums;

public enum SprintStatus
{
    [Display(Name = "Planned")]
    Planned = 0,

    [Display(Name = "Active")]
    Active = 1,

    [Display(Name = "Completed")]
    Completed = 2
}
