using BugCycle.Application.DTOs.Dashboard;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Mapping;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IIssueRepository _issues;
    private readonly IProjectService _projects;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(
        IIssueRepository issues,
        IProjectService projects,
        ICurrentUserService currentUser)
    {
        _issues = issues;
        _projects = projects;
        _currentUser = currentUser;
    }

    public async Task<DashboardDto> GetForCurrentUserAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var assigned = await _issues.ListAssignedToUserAsync(userId, ct);
        var counts = await _issues.CountByStatusForUserAsync(userId, ct);
        var projects = await _projects.ListForCurrentUserAsync(includeArchived: false, ct);

        var full = Enum.GetValues<IssueStatus>()
            .ToDictionary(s => s, s => counts.TryGetValue(s, out var c) ? c : 0);

        return new DashboardDto
        {
            MyIssues = assigned.ToDtos(),
            CountsByStatus = full,
            MyProjects = projects
        };
    }
}
