using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.DTOs.Sprints;
using BugCycle.Application.Authorization;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Mapping;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;
using BugCycle.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BugCycle.Application.Services;

public class SprintService : ISprintService
{
    private readonly ISprintRepository _sprints;
    private readonly IIssueRepository _issues;
    private readonly IProjectRepository _projects;
    private readonly IProjectAccessGuard _guard;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SprintService> _logger;

    public SprintService(
        ISprintRepository sprints,
        IIssueRepository issues,
        IProjectRepository projects,
        IProjectAccessGuard guard,
        IUnitOfWork unitOfWork,
        ILogger<SprintService> logger)
    {
        _sprints = sprints;
        _issues = issues;
        _projects = projects;
        _guard = guard;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<SprintDto>> ListForProjectAsync(int projectId, CancellationToken ct = default)
    {
        await _guard.EnsureCanViewAsync(projectId, ct);

        var sprints = await _sprints.ListForProjectAsync(projectId, ct);
        var issues = await _issues.ListAsync(new IssueFilter { ProjectId = projectId }, ct);

        var bySprint = issues
            .Where(i => i.SprintId.HasValue)
            .GroupBy(i => i.SprintId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        return sprints
            .Select(s => s.ToDto(bySprint.TryGetValue(s.Id, out var list) ? list : new List<Issue>()))
            .ToList();
    }

    public async Task<SprintDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var sprint = await _sprints.GetWithIssuesAsync(id, ct)
            ?? throw new NotFoundException("Sprintul", id);

        await _guard.EnsureCanViewAsync(sprint.ProjectId, ct);

        return sprint.ToDto();
    }

    public async Task<SprintDto?> GetActiveForProjectAsync(int projectId, CancellationToken ct = default)
    {
        await _guard.EnsureCanViewAsync(projectId, ct);

        var active = await _sprints.GetActiveForProjectAsync(projectId, ct);

        if (active is null)
        {
            return null;
        }

        var sprintIssues = await _issues.ListAsync(
            new IssueFilter { ProjectId = projectId, SprintId = active.Id },
            ct);

        return active.ToDto(sprintIssues);
    }

    public async Task<SprintDto> CreateAsync(
        int projectId,
        SaveSprintDto dto,
        CancellationToken ct = default)
    {
        await _guard.EnsureCanManageSprintsAsync(projectId, ct);

        var project = await _projects.GetByIdAsync(projectId, ct)
            ?? throw new NotFoundException("Proiectul", projectId);

        if (project.IsArchived)
        {
            throw new DomainException("Nu se pot crea sprinturi intr-un proiect arhivat.");
        }

        var sprint = new Sprint
        {
            ProjectId = projectId,
            Name = dto.Name.Trim(),
            Goal = string.IsNullOrWhiteSpace(dto.Goal) ? null : dto.Goal.Trim(),
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Status = SprintStatus.Planned
        };

        await _sprints.AddAsync(sprint, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Sprint creat: {SprintName} (id {SprintId}) in proiectul {ProjectId}.",
            sprint.Name, sprint.Id, projectId);

        return sprint.ToDto(new List<Issue>());
    }

    public async Task<SprintDto> UpdateAsync(int id, SaveSprintDto dto, CancellationToken ct = default)
    {
        var sprint = await _sprints.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Sprintul", id);

        await _guard.EnsureCanManageSprintsAsync(sprint.ProjectId, ct);

        if (sprint.Status == SprintStatus.Completed)
        {
            throw new DomainException("Un sprint incheiat nu mai poate fi modificat.");
        }

        sprint.Name = dto.Name.Trim();
        sprint.Goal = string.IsNullOrWhiteSpace(dto.Goal) ? null : dto.Goal.Trim();
        sprint.StartDate = dto.StartDate.Date;
        sprint.EndDate = dto.EndDate.Date;

        _sprints.Update(sprint);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Sprint actualizat: {SprintId}.", id);

        return await ReloadAsync(id, ct);
    }

    public async Task<SprintDto> StartAsync(int id, CancellationToken ct = default)
    {
        var sprint = await _sprints.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Sprintul", id);

        await _guard.EnsureCanManageSprintsAsync(sprint.ProjectId, ct);

        if (sprint.Status != SprintStatus.Planned)
        {
            throw new DomainException("Doar un sprint planificat poate fi pornit.");
        }

        if (await _sprints.HasActiveSprintAsync(sprint.ProjectId, id, ct))
        {
            throw new DomainException(
                "Proiectul are deja un sprint activ. Incheie-l inainte sa pornesti altul.");
        }

        sprint.Status = SprintStatus.Active;

        _sprints.Update(sprint);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Sprintul {SprintId} a fost pornit.", id);

        return await ReloadAsync(id, ct);
    }

    public async Task<SprintDto> CompleteAsync(int id, CancellationToken ct = default)
    {
        var sprint = await _sprints.GetWithIssuesAsync(id, ct)
            ?? throw new NotFoundException("Sprintul", id);

        await _guard.EnsureCanManageSprintsAsync(sprint.ProjectId, ct);

        if (sprint.Status != SprintStatus.Active)
        {
            throw new DomainException("Doar un sprint activ poate fi incheiat.");
        }

        var carriedOver = sprint.Issues.Where(i => i.Status != IssueStatus.Done).ToList();

        foreach (var issue in carriedOver)
        {
            issue.SprintId = null;
            issue.Status = IssueStatus.ToDo;
            issue.ClosedAt = null;
            issue.UpdatedAt = DateTime.UtcNow;
            _issues.Update(issue);
        }

        sprint.Status = SprintStatus.Completed;
        _sprints.Update(sprint);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Sprintul {SprintId} a fost incheiat; {Count} issue-uri s-au intors in backlog.",
            id, carriedOver.Count);

        return await ReloadAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var sprint = await _sprints.GetWithIssuesAsync(id, ct)
            ?? throw new NotFoundException("Sprintul", id);

        await _guard.EnsureCanManageSprintsAsync(sprint.ProjectId, ct);

        foreach (var issue in sprint.Issues.ToList())
        {
            issue.SprintId = null;
            issue.Status = IssueStatus.ToDo;
            issue.ClosedAt = null;
            issue.UpdatedAt = DateTime.UtcNow;
            _issues.Update(issue);
        }

        var movedBack = sprint.Issues.Count;

        _sprints.Remove(sprint);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogWarning(
            "Sprintul {SprintId} a fost sters; {Count} issue-uri s-au intors in backlog.",
            id, movedBack);
    }

    public async Task<List<IssueDto>> ListBoardIssuesAsync(int sprintId, CancellationToken ct = default)
    {
        var sprint = await _sprints.GetByIdAsync(sprintId, ct)
            ?? throw new NotFoundException("Sprintul", sprintId);

        await _guard.EnsureCanViewAsync(sprint.ProjectId, ct);

        var issues = await _issues.ListAsync(
            new IssueFilter { ProjectId = sprint.ProjectId, SprintId = sprintId },
            ct);

        return issues
            .OrderBy(i => i.BoardOrder)
            .ThenBy(i => i.Id)
            .ToDtos();
    }

    private async Task<SprintDto> ReloadAsync(int id, CancellationToken ct)
    {
        var reloaded = await _sprints.GetWithIssuesAsync(id, ct)
            ?? throw new NotFoundException("Sprintul", id);

        return reloaded.ToDto();
    }
}
