using BugCycle.Domain.Entities;

namespace BugCycle.Application.Interfaces.Repositories;

public interface ISprintRepository : IRepository<Sprint>
{
    Task<List<Sprint>> ListForProjectAsync(int projectId, CancellationToken ct = default);

    Task<Sprint?> GetActiveForProjectAsync(int projectId, CancellationToken ct = default);

    Task<Sprint?> GetWithIssuesAsync(int id, CancellationToken ct = default);

    Task<bool> HasActiveSprintAsync(int projectId, int? excludeSprintId = null, CancellationToken ct = default);
}
