using System.ComponentModel.DataAnnotations;
using BugCycle.Application.DTOs.Users;

namespace BugCycle.Application.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public UserSummaryDto Author { get; set; } = new();
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool WasEdited => UpdatedAt.HasValue;

    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class SaveCommentDto
{
    [Required(ErrorMessage = "Comentariul nu poate fi gol.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Comentariul nu poate depasi 2000 de caractere.")]
    [Display(Name = "Comentariu")]
    public string Body { get; set; } = string.Empty;
}
