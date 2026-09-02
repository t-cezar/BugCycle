using System.ComponentModel.DataAnnotations;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.DTOs.Sprints;

public class SprintDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatus Status { get; set; }

    public int IssueCount { get; set; }
    public int DoneCount { get; set; }
    public int TotalStoryPoints { get; set; }
    public int DoneStoryPoints { get; set; }

    public int DaysRemaining => (EndDate.Date - DateTime.UtcNow.Date).Days;
}

public class SaveSprintDto : IValidatableObject
{
    [Required(ErrorMessage = "Numele sprintului este obligatoriu.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Numele trebuie sa aiba intre 3 si 120 de caractere.")]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Obiectivul nu poate depasi 500 de caractere.")]
    [Display(Name = "Obiectiv")]
    public string? Goal { get; set; }

    [Required(ErrorMessage = "Data de inceput este obligatorie.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de inceput")]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [Required(ErrorMessage = "Data de sfarsit este obligatorie.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de sfarsit")]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddDays(14);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate.Date <= StartDate.Date)
        {
            yield return new ValidationResult(
                "Data de sfarsit trebuie sa fie ulterioara datei de inceput.",
                new[] { nameof(EndDate) });
        }
    }
}
