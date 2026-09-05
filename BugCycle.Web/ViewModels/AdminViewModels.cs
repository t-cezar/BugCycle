using BugCycle.Application.DTOs.Labels;
using BugCycle.Application.DTOs.Users;

namespace BugCycle.Web.ViewModels;

public class AdminUsersViewModel
{
    public List<UserWithRolesDto> Users { get; set; } = new();
    public IReadOnlyList<string> AllRoles { get; set; } = new List<string>();
    public string? CurrentUserId { get; set; }
}

public class AdminLabelsViewModel
{
    public List<LabelDto> Labels { get; set; } = new();
    public SaveLabelDto NewLabel { get; set; } = new();
    public int? EditingLabelId { get; set; }
}

public class AdminProjectsViewModel
{
    public List<BugCycle.Application.DTOs.Projects.ProjectDto> Projects { get; set; } = new();
    public bool IncludeArchived { get; set; }
}
