using System.ComponentModel.DataAnnotations;

namespace BugCycle.Domain.Enums;

public enum IssueStatus
{
    [Display(Name = "To Do")]
    ToDo = 0,

    [Display(Name = "In Progress")]
    InProgress = 1,

    [Display(Name = "In Review")]
    InReview = 2,

    [Display(Name = "Done")]
    Done = 3
}
