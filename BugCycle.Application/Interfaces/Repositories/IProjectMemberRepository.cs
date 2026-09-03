using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;

namespace BugCycle.Application.Interfaces.Repositories;

public interface IProjectMemberRepository
{
    Task<ProjectMember?> GetAsync(int projectId, string userId, CancellationToken ct = default);

    Task<ProjectRole?> GetRoleAsync(int projectId, string userId, CancellationToken ct = default);

    Task<List<ProjectMember>> ListForProjectAsync(int projectId, CancellationToken ct = default);

    Task<int> CountOwnersAsync(int projectId, CancellationToken ct = default);

    Task<Dictionary<string, int>> CountProjectsPerUserAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default);

    Task AddAsync(ProjectMember member, CancellationToken ct = default);

    void Update(ProjectMember member);

    void Remove(ProjectMember member);
}
