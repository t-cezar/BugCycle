using BugCycle.Application.Authorization;
using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.DTOs.Labels;
using BugCycle.Application.DTOs.Projects;
using BugCycle.Application.DTOs.Sprints;
using BugCycle.Application.DTOs.Users;

namespace BugCycle.Web.ViewModels;

public class ProjectListViewModel
{
    public List<ProjectDto> Projects { get; set; } = new();
    public bool IncludeArchived { get; set; }
}

// Baza pentru cele trei pagini ale unui proiect (backlog, sprinturi, board),
// ca sa poata partaja acelasi header si aceeasi navigare intre tab-uri.

public abstract class ProjectPageViewModel
{
    public ProjectDetailDto Project { get; set; } = new();
    public ProjectPermissions Permissions { get; set; } = ProjectPermissions.None(0);
}

public class BacklogViewModel : ProjectPageViewModel
{
    public List<IssueDto> Issues { get; set; } = new();
    public List<SprintDto> OpenSprints { get; set; } = new();
    public List<UserSummaryDto> AssignableUsers { get; set; } = new();
    public List<LabelDto> Labels { get; set; } = new();
    public IssueFilter Filter { get; set; } = new();

    //Cand e true se afiseaza doar issue-urile fara sprint
    public bool ShowBacklogOnly { get; set; } = true;
}

public class SprintsViewModel : ProjectPageViewModel
{
    public List<SprintDto> Sprints { get; set; } = new();
    public SaveSprintDto NewSprint { get; set; } = new();
}

public class BoardViewModel : ProjectPageViewModel
{
    public SprintDto? ActiveSprint { get; set; }
}

public class MembersViewModel : ProjectPageViewModel
{
    public List<ProjectMemberDto> Members { get; set; } = new();
    public AddProjectMemberDto NewMember { get; set; } = new();
}
