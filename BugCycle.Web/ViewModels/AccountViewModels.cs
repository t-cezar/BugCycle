using System.ComponentModel.DataAnnotations;

namespace BugCycle.Web.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Numele complet este obligatoriu.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Numele trebuie sa aiba intre 3 si 120 de caractere.")]
    [Display(Name = "Nume complet")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresa de email este obligatorie.")]
    [EmailAddress(ErrorMessage = "Adresa de email nu este valida.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola trebuie sa aiba cel putin 8 caractere.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirma parola")]
    [Compare(nameof(Password), ErrorMessage = "Cele doua parole nu coincid.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Adresa de email este obligatorie.")]
    [EmailAddress(ErrorMessage = "Adresa de email nu este valida.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Tine-ma minte")]
    public bool RememberMe { get; set; } = true;

    public string? ReturnUrl { get; set; }
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Numele complet este obligatoriu.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Numele trebuie sa aiba intre 3 si 120 de caractere.")]
    [Display(Name = "Nume complet")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public string Initials { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public int ProjectCount { get; set; }
    public int AssignedIssueCount { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Parola actuala este obligatorie.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola actuala")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Parola noua este obligatorie.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola trebuie sa aiba cel putin 8 caractere.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parola noua")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirma parola noua")]
    [Compare(nameof(NewPassword), ErrorMessage = "Cele doua parole nu coincid.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
