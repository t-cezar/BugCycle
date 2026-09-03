using BugCycle.Application.Authorization;
using BugCycle.Application.DTOs.Projects;
using BugCycle.Application.DTOs.Users;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Mapping;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;
using BugCycle.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BugCycle.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IProjectMemberRepository _members;
    private readonly IIssueRepository _issues;
    private readonly IUserRepository _users;
    private readonly IProjectAccessGuard _guard;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IProjectRepository projects,
        IProjectMemberRepository members,
        IIssueRepository issues,
        IUserRepository users,
        IProjectAccessGuard guard,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<ProjectService> logger)
    {
        _projects = projects;
        _members = members;
        _issues = issues;
        _users = users;
        _guard = guard;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<ProjectDto>> ListForCurrentUserAsync(
        bool includeArchived,
        CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var projects = await _projects.ListForUserAsync(userId, includeArchived, ct);
        var counts = await _projects.CountIssuesPerProjectAsync(projects.Select(p => p.Id), ct);

        return projects
            .Select(p => p.ToDto(
                myRole: p.Members.FirstOrDefault(m => m.UserId == userId)?.Role,
                issueCount: counts.TryGetValue(p.Id, out var c) ? c : 0))
            .ToList();
    }

    public async Task<List<ProjectDto>> ListAllForAdminAsync(
        bool includeArchived,
        CancellationToken ct = default)
    {
        var userId = _currentUser.UserId;
        var projects = await _projects.ListAllWithCreatorAsync(includeArchived, ct);
        var counts = await _projects.CountIssuesPerProjectAsync(projects.Select(p => p.Id), ct);

        return projects
            .Select(p => p.ToDto(
                myRole: userId is null
                    ? null
                    : p.Members.FirstOrDefault(m => m.UserId == userId)?.Role,
                issueCount: counts.TryGetValue(p.Id, out var c) ? c : 0))
            .ToList();
    }

    public async Task<ProjectDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var permissions = await _guard.EnsureCanViewAsync(id, ct);

        var project = await _projects.GetWithMembersAsync(id, ct)
            ?? throw new NotFoundException("Proiectul", id);

        var counts = await _projects.CountIssuesPerProjectAsync(new[] { id }, ct);

        return project.ToDetailDto(
            myRole: permissions.Role,
            issueCount: counts.TryGetValue(id, out var c) ? c : 0);
    }

    public async Task<ProjectDto> CreateAsync(SaveProjectDto dto, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var key = NormalizeKey(dto.Key);

        if (await _projects.KeyExistsAsync(key, null, ct))
        {
            throw new DomainException($"Prefixul '{key}' este deja folosit de alt proiect.");
        }

        var project = new Project
        {
            Key = key,
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow,
            IsArchived = false
        };

        project.Members.Add(new ProjectMember
        {
            UserId = userId,
            Role = ProjectRole.Owner,
            JoinedAt = DateTime.UtcNow
        });

        await _projects.AddAsync(project, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Proiect creat: {ProjectKey} (id {ProjectId}) de utilizatorul {UserId}",
            project.Key, project.Id, userId);

        return project.ToDto(myRole: ProjectRole.Owner, issueCount: 0);
    }

    public async Task<ProjectDto> UpdateAsync(int id, SaveProjectDto dto, CancellationToken ct = default)
    {
        var permissions = await _guard.EnsureCanEditProjectAsync(id, ct);

        var project = await _projects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Proiectul", id);

        var key = NormalizeKey(dto.Key);

        if (await _projects.KeyExistsAsync(key, id, ct))
        {
            throw new DomainException($"Prefixul '{key}' este deja folosit de alt proiect.");
        }

        project.Key = key;
        project.Name = dto.Name.Trim();
        project.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();

        _projects.Update(project);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Proiect actualizat: {ProjectId}", id);

        var counts = await _projects.CountIssuesPerProjectAsync(new[] { id }, ct);

        return project.ToDto(
            myRole: permissions.Role,
            issueCount: counts.TryGetValue(id, out var c) ? c : 0);
    }

    public async Task SetArchivedAsync(int id, bool archived, CancellationToken ct = default)
    {
        await _guard.EnsureCanEditProjectAsync(id, ct);

        var project = await _projects.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Proiectul", id);

        project.IsArchived = archived;

        _projects.Update(project);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Proiectul {ProjectId} a fost {State}.",
            id, archived ? "arhivat" : "dezarhivat");
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await _guard.EnsureCanEditProjectAsync(id, ct);

        var project = await _projects.GetForDeletionAsync(id, ct)
            ?? throw new NotFoundException("Proiectul", id);

        var issueCount = project.Issues.Count;

        _issues.RemoveRange(project.Issues);
        _projects.Remove(project);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Proiectul {ProjectKey} (id {ProjectId}) a fost sters impreuna cu {IssueCount} issue-uri.",
            project.Key, id, issueCount);
    }

    public async Task<List<ProjectMemberDto>> ListMembersAsync(
        int projectId,
        CancellationToken ct = default)
    {
        await _guard.EnsureCanViewAsync(projectId, ct);

        var members = await _members.ListForProjectAsync(projectId, ct);
        return members.ToDtos();
    }

    public async Task<ProjectMemberDto> AddMemberAsync(
        int projectId,
        AddProjectMemberDto dto,
        CancellationToken ct = default)
    {
        await _guard.EnsureCanManageMembersAsync(projectId, ct);

        _ = await _projects.GetByIdAsync(projectId, ct)
            ?? throw new NotFoundException("Proiectul", projectId);

        var user = await _users.GetByEmailAsync(dto.Email, ct)
            ?? throw new NotFoundException(
                $"Nu exista niciun utilizator inregistrat cu adresa '{dto.Email}'.");

        if (await _members.GetAsync(projectId, user.Id, ct) is not null)
        {
            throw new DomainException($"{user.FullName} este deja membru al acestui proiect.");
        }

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = user.Id,
            Role = dto.Role,
            JoinedAt = DateTime.UtcNow
        };

        await _members.AddAsync(member, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Utilizatorul {UserId} a fost adaugat in proiectul {ProjectId} cu rolul {Role}.",
            user.Id, projectId, dto.Role);

        member.User = user;
        return member.ToDto();
    }

    public async Task<ProjectMemberDto> UpdateMemberRoleAsync(
        int projectId,
        string userId,
        UpdateProjectMemberRoleDto dto,
        CancellationToken ct = default)
    {
        await _guard.EnsureCanManageMembersAsync(projectId, ct);

        var member = await _members.GetAsync(projectId, userId, ct)
            ?? throw new NotFoundException("Membrul proiectului", userId);

        if (member.Role == dto.Role)
        {
            return member.ToDto();
        }

        if (member.Role == ProjectRole.Owner && await _members.CountOwnersAsync(projectId, ct) <= 1)
        {
            throw new DomainException(
                "Nu poti schimba rolul ultimului Owner. Promoveaza mai intai un alt membru la Owner.");
        }

        if (dto.Role == ProjectRole.Viewer)
        {
            await UnassignIssuesAsync(projectId, userId, ct);
        }

        member.Role = dto.Role;
        _members.Update(member);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Rolul utilizatorului {UserId} in proiectul {ProjectId} a devenit {Role}.",
            userId, projectId, dto.Role);

        return member.ToDto();
    }

    public async Task RemoveMemberAsync(int projectId, string userId, CancellationToken ct = default)
    {
        await _guard.EnsureCanManageMembersAsync(projectId, ct);

        var member = await _members.GetAsync(projectId, userId, ct)
            ?? throw new NotFoundException("Membrul proiectului", userId);

        if (member.Role == ProjectRole.Owner && await _members.CountOwnersAsync(projectId, ct) <= 1)
        {
            throw new DomainException(
                "Nu poti scoate ultimul Owner al proiectului. Promoveaza mai intai un alt membru.");
        }

        var unassigned = await UnassignIssuesAsync(projectId, userId, ct);

        _members.Remove(member);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Utilizatorul {UserId} a fost scos din proiectul {ProjectId}; {Count} issue-uri au fost dezasignate.",
            userId, projectId, unassigned);
    }

    public async Task<List<UserSummaryDto>> ListAssignableUsersAsync(
        int projectId,
        CancellationToken ct = default)
    {
        await _guard.EnsureCanViewAsync(projectId, ct);

        var members = await _members.ListForProjectAsync(projectId, ct);

        return members
            .Where(m => m.Role is ProjectRole.Owner or ProjectRole.Member)
            .Where(m => m.User is not null)
            .Select(m => m.User.ToSummaryDto())
            .OrderBy(u => u.FullName)
            .ToList();
    }

    public Task<ProjectPermissions> GetPermissionsAsync(int projectId, CancellationToken ct = default)
        => _guard.GetPermissionsAsync(projectId, ct);

    private async Task<int> UnassignIssuesAsync(int projectId, string userId, CancellationToken ct)
    {
        var assigned = await _issues.ListAssignedInProjectAsync(projectId, userId, ct);

        foreach (var issue in assigned)
        {
            issue.AssigneeId = null;
            issue.UpdatedAt = DateTime.UtcNow;
            _issues.Update(issue);
        }

        return assigned.Count;
    }

    private static string NormalizeKey(string key) => key.Trim().ToUpperInvariant();
}
