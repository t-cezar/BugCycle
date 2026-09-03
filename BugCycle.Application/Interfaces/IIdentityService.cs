namespace BugCycle.Application.Interfaces;

public interface IIdentityService
{
    Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken ct = default);

    Task<Dictionary<string, List<string>>> GetRolesForUsersAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetAllRolesAsync(CancellationToken ct = default);

    Task AddToRoleAsync(string userId, string role, CancellationToken ct = default);

    Task RemoveFromRoleAsync(string userId, string role, CancellationToken ct = default);

    Task<int> CountUsersInRoleAsync(string role, CancellationToken ct = default);
}
