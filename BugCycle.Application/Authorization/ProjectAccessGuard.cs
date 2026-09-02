// Todo: Interfaces, repsoitories, services, etc.


using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Enums;
using BugCycle.Domain.Exceptions;

namespace BugCycle.Application.Authorization;

public class ProjectAccessGuard : IProjectAccessGuard
{
    private readonly ICurrentUserService _currentUser;
    private readonly IProjectMemberRepository _members;

    public ProjectAccessGuard(
        ICurrentUserService currentUser,
        IProjectMemberRepository members)
    {
        _currentUser = currentUser;
        _members = members;
    }

    public async Task<ProjectPermissions> GetPermissionsAsync(
        int projectId,
        CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return ProjectPermissions.None(projectId);
        }

        // Administratorul de sistem trece peste rolurile de proiect. Fara
        // aceasta linie un admin nu ar putea interveni in proiectele altora si
        // panoul de administrare ar parea nefunctional.
        if (_currentUser.IsAdmin)
        {
            var adminRole = await _members.GetRoleAsync(projectId, _currentUser.RequireUserId(), ct);

            return new ProjectPermissions
            {
                ProjectId = projectId,
                Role = adminRole,
                IsSystemAdmin = true,
                CanView = true,
                CanEditIssues = true,
                CanComment = true,
                CanManageSprints = true,
                CanManageMembers = true,
                CanDeleteIssues = true,
                CanEditProject = true
            };
        }

        var role = await _members.GetRoleAsync(projectId, _currentUser.RequireUserId(), ct);

        if (role is null)
        {
            return ProjectPermissions.None(projectId);
        }

        var isOwner = role == ProjectRole.Owner;
        var canWrite = role is ProjectRole.Owner or ProjectRole.Member;

        return new ProjectPermissions
        {
            ProjectId = projectId,
            Role = role,
            IsSystemAdmin = false,
            CanView = true,
            CanEditIssues = canWrite,
            CanComment = canWrite,
            CanManageSprints = isOwner,
            CanManageMembers = isOwner,
            CanDeleteIssues = isOwner,
            CanEditProject = isOwner
        };
    }

    public Task<ProjectPermissions> EnsureCanViewAsync(int projectId, CancellationToken ct = default)
        => EnsureAsync(projectId, p => p.CanView, "Nu ai acces la acest proiect.", ct);

    public Task<ProjectPermissions> EnsureCanEditIssuesAsync(int projectId, CancellationToken ct = default)
        => EnsureAsync(
            projectId,
            p => p.CanEditIssues,
            "Nu ai permisiunea sa modifici issue-uri in acest proiect.",
            ct);

    public Task<ProjectPermissions> EnsureCanCommentAsync(int projectId, CancellationToken ct = default)
        => EnsureAsync(
            projectId,
            p => p.CanComment,
            "Nu ai permisiunea sa comentezi in acest proiect.",
            ct);

    public Task<ProjectPermissions> EnsureCanManageSprintsAsync(int projectId, CancellationToken ct = default)
        => EnsureAsync(
            projectId,
            p => p.CanManageSprints,
            "Doar un Owner de proiect poate gestiona sprinturile.",
            ct);

    public Task<ProjectPermissions> EnsureCanManageMembersAsync(int projectId, CancellationToken ct = default)
        => EnsureAsync(
            projectId,
            p => p.CanManageMembers,
            "Doar un Owner de proiect poate gestiona membrii.",
            ct);

    public Task<ProjectPermissions> EnsureCanDeleteIssuesAsync(int projectId, CancellationToken ct = default)
        => EnsureAsync(
            projectId,
            p => p.CanDeleteIssues,
            "Doar un Owner de proiect poate sterge issue-uri.",
            ct);

    public Task<ProjectPermissions> EnsureCanEditProjectAsync(int projectId, CancellationToken ct = default)
        => EnsureAsync(
            projectId,
            p => p.CanEditProject,
            "Doar un Owner de proiect poate modifica setarile proiectului.",
            ct);

    private async Task<ProjectPermissions> EnsureAsync(
        int projectId,
        Func<ProjectPermissions, bool> predicate,
        string message,
        CancellationToken ct)
    {
        var permissions = await GetPermissionsAsync(projectId, ct);

        if (!predicate(permissions))
        {
            throw new ForbiddenException(message);
        }

        return permissions;
    }
}
