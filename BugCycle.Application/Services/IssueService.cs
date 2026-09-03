using BugCycle.Application.Authorization;
using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Mapping;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;
using BugCycle.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BugCycle.Application.Services;

public class IssueService : IIssueService
{
    private readonly IIssueRepository _issues;
    private readonly IProjectRepository _projects;
    private readonly IProjectMemberRepository _members;
    private readonly ISprintRepository _sprints;
    private readonly ILabelRepository _labels;
    private readonly IProjectAccessGuard _guard;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IssueService> _logger;

    public IssueService(
        IIssueRepository issues,
        IProjectRepository projects,
        IProjectMemberRepository members,
        ISprintRepository sprints,
        ILabelRepository labels,
        IProjectAccessGuard guard,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<IssueService> logger)
    {
        _issues = issues;
        _projects = projects;
        _members = members;
        _sprints = sprints;
        _labels = labels;
        _guard = guard;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<IssueDto>> ListAsync(IssueFilter filter, CancellationToken ct = default)
    {
        if (filter.ProjectId is null)
        {
            throw new DomainException("Filtrarea issue-urilor necesita un proiect.");
        }

        await _guard.EnsureCanViewAsync(filter.ProjectId.Value, ct);

        var issues = await _issues.ListAsync(filter, ct);
        return issues.ToDtos();
    }

    public async Task<IssueDetailDto> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var issue = await _issues.GetDetailAsync(id, ct)
            ?? throw new NotFoundException("Issue-ul", id);

        var permissions = await _guard.EnsureCanViewAsync(issue.ProjectId, ct);

        return issue.ToDetailDto(
            currentUserId: _currentUser.UserId,
            canEdit: permissions.CanEditIssues,
            canDelete: permissions.CanDeleteIssues,
            canComment: permissions.CanComment,
            currentUserIsProjectOwner: permissions.IsOwner);
    }

    public async Task<IssueDto> CreateAsync(CreateIssueDto dto, CancellationToken ct = default)
    {
        await _guard.EnsureCanEditIssuesAsync(dto.ProjectId, ct);

        var project = await _projects.GetByIdAsync(dto.ProjectId, ct)
            ?? throw new NotFoundException("Proiectul", dto.ProjectId);

        if (project.IsArchived)
        {
            throw new DomainException("Nu se pot crea issue-uri intr-un proiect arhivat.");
        }

        await ValidateSprintAsync(dto.ProjectId, dto.SprintId, ct);
        await ValidateAssigneeAsync(dto.ProjectId, dto.AssigneeId, ct);

        var now = DateTime.UtcNow;

        var issue = new Issue
        {
            ProjectId = dto.ProjectId,
            Number = await _issues.GetNextNumberAsync(dto.ProjectId, ct),
            SprintId = dto.SprintId,
            Title = dto.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Type = dto.Type,
            Status = IssueStatus.ToDo,
            Priority = dto.Priority,
            ReporterId = _currentUser.RequireUserId(),
            AssigneeId = dto.AssigneeId,
            StoryPoints = dto.StoryPoints,
            BoardOrder = await _issues.GetMaxBoardOrderAsync(dto.SprintId, IssueStatus.ToDo, ct) + 1,
            CreatedAt = now,
            UpdatedAt = now
        };

        await ApplyLabelsAsync(issue, dto.LabelIds, ct);

        await _issues.AddAsync(issue, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Issue creat: {ProjectKey}-{Number} (id {IssueId}) in proiectul {ProjectId}.",
            project.Key, issue.Number, issue.Id, dto.ProjectId);

        return await ReloadDtoAsync(issue.Id, ct);
    }

    public async Task<IssueDto> UpdateAsync(int id, UpdateIssueDto dto, CancellationToken ct = default)
    {
        var issue = await _issues.GetWithLabelsAsync(id, ct)
            ?? throw new NotFoundException("Issue-ul", id);

        await _guard.EnsureCanEditIssuesAsync(issue.ProjectId, ct);

        await ValidateSprintAsync(issue.ProjectId, dto.SprintId, ct);
        await ValidateAssigneeAsync(issue.ProjectId, dto.AssigneeId, ct);

        var effectiveAssignee = dto.AssigneeId;

        if (dto.Status == IssueStatus.Done && string.IsNullOrEmpty(effectiveAssignee))
        {
            throw new DomainException(
                "Un issue nu poate fi mutat in Done fara sa aiba un assignee.");
        }

        var statusChanged = issue.Status != dto.Status;
        var sprintChanged = issue.SprintId != dto.SprintId;

        issue.Title = dto.Title.Trim();
        issue.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        issue.Type = dto.Type;
        issue.Priority = dto.Priority;
        issue.AssigneeId = string.IsNullOrEmpty(effectiveAssignee) ? null : effectiveAssignee;
        issue.StoryPoints = dto.StoryPoints;
        issue.SprintId = dto.SprintId;
        issue.Status = dto.Status;
        issue.UpdatedAt = DateTime.UtcNow;

        ApplyClosedAt(issue, dto.Status);

        if (statusChanged || sprintChanged)
        {
            issue.BoardOrder = await _issues.GetMaxBoardOrderAsync(dto.SprintId, dto.Status, ct) + 1;
        }

        await ApplyLabelsAsync(issue, dto.LabelIds, ct);

        _issues.Update(issue);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Issue actualizat: {IssueId}.", id);

        return await ReloadDtoAsync(id, ct);
    }

    public async Task<IssueDto> MoveAsync(int id, MoveIssueDto dto, CancellationToken ct = default)
    {
        var issue = await _issues.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Issue-ul", id);

        await _guard.EnsureCanEditIssuesAsync(issue.ProjectId, ct);

        if (dto.Status == IssueStatus.Done && string.IsNullOrEmpty(issue.AssigneeId))
        {
            throw new DomainException(
                "Asigneaza issue-ul cuiva inainte sa il muti in Done.");
        }

        var previousStatus = issue.Status;
        issue.Status = dto.Status;
        issue.UpdatedAt = DateTime.UtcNow;

        ApplyClosedAt(issue, dto.Status);

        _issues.Update(issue);

        await ReorderColumnAsync(issue, dto.Position, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Issue {IssueId} mutat din {OldStatus} in {NewStatus}, pozitia {Position}.",
            id, previousStatus, dto.Status, dto.Position);

        return await ReloadDtoAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var issue = await _issues.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Issue-ul", id);

        await _guard.EnsureCanDeleteIssuesAsync(issue.ProjectId, ct);

        _issues.Remove(issue);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogWarning("Issue-ul {IssueId} din proiectul {ProjectId} a fost sters.", id, issue.ProjectId);
    }

    public async Task<IssueDto> SetSprintAsync(int id, int? sprintId, CancellationToken ct = default)
    {
        var issue = await _issues.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Issue-ul", id);

        await _guard.EnsureCanEditIssuesAsync(issue.ProjectId, ct);
        await ValidateSprintAsync(issue.ProjectId, sprintId, ct);

        if (issue.SprintId == sprintId)
        {
            return await ReloadDtoAsync(id, ct);
        }

        issue.SprintId = sprintId;

        issue.Status = IssueStatus.ToDo;
        issue.ClosedAt = null;
        issue.UpdatedAt = DateTime.UtcNow;
        issue.BoardOrder = await _issues.GetMaxBoardOrderAsync(sprintId, IssueStatus.ToDo, ct) + 1;

        _issues.Update(issue);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Issue-ul {IssueId} a fost mutat in sprintul {SprintId}.",
            id, sprintId?.ToString() ?? "backlog");

        return await ReloadDtoAsync(id, ct);
    }

    private async Task ApplyLabelsAsync(Issue issue, List<int> labelIds, CancellationToken ct)
    {
        var wanted = labelIds.Distinct().ToList();

        if (wanted.Count > 0)
        {
            var existingLabels = await _labels.GetByIdsAsync(wanted, ct);

            if (existingLabels.Count != wanted.Count)
            {
                var missing = wanted.Except(existingLabels.Select(l => l.Id));
                throw new NotFoundException(
                    $"Etichetele cu identificatorii {string.Join(", ", missing)} nu exista.");
            }
        }

        var current = issue.IssueLabels.Select(il => il.LabelId).ToList();

        foreach (var toRemove in issue.IssueLabels.Where(il => !wanted.Contains(il.LabelId)).ToList())
        {
            issue.IssueLabels.Remove(toRemove);
        }

        foreach (var labelId in wanted.Where(id => !current.Contains(id)))
        {
            issue.IssueLabels.Add(new IssueLabel { IssueId = issue.Id, LabelId = labelId });
        }
    }

    private async Task ReorderColumnAsync(Issue moved, int desiredPosition, CancellationToken ct)
    {
        var column = await _issues.ListColumnAsync(moved.SprintId, moved.Status, ct);

        var ordered = column.Where(i => i.Id != moved.Id).ToList();

        var index = Math.Clamp(desiredPosition, 0, ordered.Count);
        ordered.Insert(index, moved);

        for (var i = 0; i < ordered.Count; i++)
        {
            if (ordered[i].BoardOrder != i)
            {
                ordered[i].BoardOrder = i;
                _issues.Update(ordered[i]);
            }
        }
    }

    private static void ApplyClosedAt(Issue issue, IssueStatus status)
    {
        if (status == IssueStatus.Done)
        {
            issue.ClosedAt ??= DateTime.UtcNow;
        }
        else
        {
            issue.ClosedAt = null;
        }
    }

    private async Task ValidateSprintAsync(int projectId, int? sprintId, CancellationToken ct)
    {
        if (sprintId is null)
        {
            return;
        }

        var sprint = await _sprints.GetByIdAsync(sprintId.Value, ct)
            ?? throw new NotFoundException("Sprintul", sprintId.Value);

        if (sprint.ProjectId != projectId)
        {
            throw new DomainException("Sprintul selectat nu apartine acestui proiect.");
        }

        if (sprint.Status == SprintStatus.Completed)
        {
            throw new DomainException("Nu poti muta issue-uri intr-un sprint deja incheiat.");
        }
    }

    private async Task ValidateAssigneeAsync(int projectId, string? assigneeId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(assigneeId))
        {
            return;
        }

        var role = await _members.GetRoleAsync(projectId, assigneeId, ct)
            ?? throw new DomainException(
                "Nu poti asigna un issue unui utilizator care nu este membru al proiectului.");

        if (role == ProjectRole.Viewer)
        {
            throw new DomainException(
                "Un membru cu rol de Viewer nu poate primi issue-uri asignate.");
        }
    }

    private async Task<IssueDto> ReloadDtoAsync(int id, CancellationToken ct)
    {
        var reloaded = await _issues.GetDetailAsync(id, ct)
            ?? throw new NotFoundException("Issue-ul", id);

        return reloaded.ToDto();
    }
}
