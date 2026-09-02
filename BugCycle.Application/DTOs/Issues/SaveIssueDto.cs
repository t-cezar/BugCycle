using System.ComponentModel.DataAnnotations;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.DTOs.Issues;

public class CreateIssueDto
{
    [Required(ErrorMessage = "Proiectul este obligatoriu.")]
    [Range(1, int.MaxValue, ErrorMessage = "Proiectul este obligatoriu.")]
    [Display(Name = "Proiect")]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Titlul trebuie sa aiba intre 5 si 200 de caractere.")]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Descrierea nu poate depasi 4000 de caractere.")]
    [Display(Name = "Descriere")]
    public string? Description { get; set; }

    [Display(Name = "Tip")]
    public IssueType Type { get; set; } = IssueType.Bug;

    [Display(Name = "Prioritate")]
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;

    [Display(Name = "Asignat lui")]
    public string? AssigneeId { get; set; }

    //Null inseamna ca issue-ul se creeaza direct in backlog
    [Display(Name = "Sprint")]
    public int? SprintId { get; set; }

    [Range(0, 100, ErrorMessage = "Story points trebuie sa fie intre 0 si 100.")]
    [Display(Name = "Story points")]
    public int? StoryPoints { get; set; }

    [Display(Name = "Etichete")]
    public List<int> LabelIds { get; set; } = new();
}

public class UpdateIssueDto
{
    [Required(ErrorMessage = "Titlul este obligatoriu.")]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Titlul trebuie sa aiba intre 5 si 200 de caractere.")]
    [Display(Name = "Titlu")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Descrierea nu poate depasi 4000 de caractere.")]
    [Display(Name = "Descriere")]
    public string? Description { get; set; }

    [Display(Name = "Tip")]
    public IssueType Type { get; set; }

    [Display(Name = "Status")]
    public IssueStatus Status { get; set; }

    [Display(Name = "Prioritate")]
    public IssuePriority Priority { get; set; }

    [Display(Name = "Asignat lui")]
    public string? AssigneeId { get; set; }

    [Display(Name = "Sprint")]
    public int? SprintId { get; set; }

    [Range(0, 100, ErrorMessage = "Story points trebuie sa fie intre 0 si 100.")]
    [Display(Name = "Story points")]
    public int? StoryPoints { get; set; }

    [Display(Name = "Etichete")]
    public List<int> LabelIds { get; set; } = new();
}

public class MoveIssueDto
{
    [Display(Name = "Status")]
    public IssueStatus Status { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Pozitia trebuie sa fie un numar pozitiv.")]
    public int Position { get; set; }
}
