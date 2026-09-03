using BugCycle.Application.DTOs.Issues;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.Interfaces.Repositories;

public interface IIssueRepository : IRepository<Issue>
{
    Task<Issue?> GetDetailAsync(int id, CancellationToken ct = default);

    Task<Issue?> GetWithLabelsAsync(int id, CancellationToken ct = default);

    Task<List<Issue>> ListAsync(IssueFilter filter, CancellationToken ct = default);

    Task<List<Issue>> ListAssignedToUserAsync(string userId, CancellationToken ct = default);

    Task<Dictionary<IssueStatus, int>> CountByStatusForUserAsync(string userId, CancellationToken ct = default);

    Task<int> GetNextNumberAsync(int projectId, CancellationToken ct = default);

    Task<int> GetMaxBoardOrderAsync(int? sprintId, IssueStatus status, CancellationToken ct = default);

    Task<List<Issue>> ListColumnAsync(int? sprintId, IssueStatus status, CancellationToken ct = default);

    Task<List<Issue>> ListAssignedInProjectAsync(int projectId, string userId, CancellationToken ct = default);
}
