using System.ComponentModel.DataAnnotations;

namespace BugCycle.Domain.Enums;

public enum ProjectRole
{
    [Display(Name = "Owner")]
    Owner = 0,

    [Display(Name = "Member")]
    Member = 1,

    [Display(Name = "Viewer")]
    Viewer = 2
}
