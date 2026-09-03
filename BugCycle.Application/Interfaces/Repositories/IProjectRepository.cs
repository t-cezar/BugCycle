using BugCycle.Domain.Entities;

namespace BugCycle.Application.Interfaces.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    Task<List<Project>> ListForUserAsync(string userId, bool includeArchived, CancellationToken ct = default);

    Task<List<Project>> ListAllWithCreatorAsync(bool includeArchived, CancellationToken ct = default);

    Task<Project?> GetWithMembersAsync(int id, CancellationToken ct = default);

    Task<Project?> GetForDeletionAsync(int id, CancellationToken ct = default);

    Task<bool> KeyExistsAsync(string key, int? excludeProjectId = null, CancellationToken ct = default);

    Task<Dictionary<int, int>> CountIssuesPerProjectAsync(IEnumerable<int> projectIds, CancellationToken ct = default);
}
