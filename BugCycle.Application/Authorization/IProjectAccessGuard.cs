namespace BugCycle.Application.Authorization;

public interface IProjectAccessGuard
{
    Task<ProjectPermissions> GetPermissionsAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> EnsureCanViewAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> EnsureCanEditIssuesAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> EnsureCanCommentAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> EnsureCanManageSprintsAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> EnsureCanManageMembersAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> EnsureCanDeleteIssuesAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> EnsureCanEditProjectAsync(int projectId, CancellationToken ct = default);
}
