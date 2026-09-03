using BugCycle.Application.DTOs.Users;

namespace BugCycle.Application.Interfaces.Services;

public interface IUserAdminService
{
    Task<List<UserWithRolesDto>> ListAsync(CancellationToken ct = default);

    Task<UserWithRolesDto> GetByIdAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<string>> ListRolesAsync(CancellationToken ct = default);

    Task<UserWithRolesDto> AddRoleAsync(string userId, string role, CancellationToken ct = default);

    Task<UserWithRolesDto> RemoveRoleAsync(string userId, string role, CancellationToken ct = default);
}
