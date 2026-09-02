using System.ComponentModel.DataAnnotations;

namespace BugCycle.Application.DTOs.Labels;

public class LabelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}

public class SaveLabelDto
{
    [Required(ErrorMessage = "Numele etichetei este obligatoriu.")]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "Numele trebuie sa aiba intre 2 si 40 de caractere.")]
    [Display(Name = "Nume")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Culoarea este obligatorie.")]
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Culoarea trebuie sa fie in format hexazecimal, de exemplu #2563eb.")]
    [Display(Name = "Culoare")]
    public string ColorHex { get; set; } = "#2563eb";
}
