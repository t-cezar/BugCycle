using BugCycle.Domain.Entities;

namespace BugCycle.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default);

    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);

    Task<List<ApplicationUser>> ListAllAsync(CancellationToken ct = default);

    Task<List<ApplicationUser>> ListByIdsAsync(IEnumerable<string> ids, CancellationToken ct = default);
}
