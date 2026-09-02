using System.ComponentModel.DataAnnotations;
using BugCycle.Application.DTOs.Users;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.DTOs.Projects;

public class ProjectDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    public int MemberCount { get; set; }
    public int IssueCount { get; set; }

    public ProjectRole? MyRole { get; set; }
}

public class ProjectDetailDto : ProjectDto
{
    public List<ProjectMemberDto> Members { get; set; } = new();
}

public class ProjectMemberDto
{
    public UserSummaryDto User { get; set; } = new();
    public ProjectRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}

public class SaveProjectDto
{
    [Required(ErrorMessage = "Prefixul proiectului este obligatoriu.")]
    [StringLength(10, MinimumLength = 2, ErrorMessage = "Prefixul trebuie sa aiba intre 2 si 10 caractere.")]
    [RegularExpression("^[A-Za-z][A-Za-z0-9]*$", ErrorMessage = "Prefixul poate contine doar litere si cifre si trebuie sa inceapa cu o litera.")]
    [Display(Name = "Prefix (ex. BUG)")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Numele proiectului este obligatoriu.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Numele trebuie sa aiba intre 3 si 120 de caractere.")]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Descrierea nu poate depasi 2000 de caractere.")]
    [Display(Name = "Descriere")]
    public string? Description { get; set; }
}

public class AddProjectMemberDto
{
    [Required(ErrorMessage = "Adresa de email este obligatorie.")]
    [EmailAddress(ErrorMessage = "Adresa de email nu este valida.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Rol in proiect")]
    public ProjectRole Role { get; set; } = ProjectRole.Member;
}

public class UpdateProjectMemberRoleDto
{
    [Display(Name = "Rol in proiect")]
    public ProjectRole Role { get; set; }
}
