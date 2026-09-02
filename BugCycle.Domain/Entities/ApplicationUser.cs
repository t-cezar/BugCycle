using Microsoft.AspNetCore.Identity;

namespace BugCycle.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    public ICollection<Issue> ReportedIssues { get; set; } = new List<Issue>();
    public ICollection<Issue> AssignedIssues { get; set; } = new List<Issue>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public string Initials
    {
        get
        {
            var source = string.IsNullOrWhiteSpace(FullName) ? (UserName ?? "?") : FullName;
            var parts = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return "?";
            }

            if (parts.Length == 1)
            {
                return parts[0].Length >= 2
                    ? parts[0][..2].ToUpperInvariant()
                    : parts[0].ToUpperInvariant();
            }

            return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
        }
    }
}
