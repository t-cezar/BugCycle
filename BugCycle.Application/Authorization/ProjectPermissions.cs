using BugCycle.Domain.Enums;

namespace BugCycle.Application.Authorization;

public class ProjectPermissions
{
    public int ProjectId { get; init; }

    public ProjectRole? Role { get; init; }

    public bool IsSystemAdmin { get; init; }

    public bool CanView { get; init; }
    public bool CanEditIssues { get; init; }
    public bool CanComment { get; init; }
    public bool CanManageSprints { get; init; }
    public bool CanManageMembers { get; init; }
    public bool CanDeleteIssues { get; init; }
    public bool CanEditProject { get; init; }

    
    public bool IsOwner => Role == ProjectRole.Owner || IsSystemAdmin;

    public static ProjectPermissions None(int projectId) => new()
    {
        ProjectId = projectId,
        Role = null,
        IsSystemAdmin = false
    };
}
