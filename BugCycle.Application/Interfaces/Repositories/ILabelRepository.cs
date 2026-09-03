using BugCycle.Domain.Entities;

namespace BugCycle.Application.Interfaces.Repositories;

public interface ILabelRepository : IRepository<Label>
{
    Task<bool> NameExistsAsync(string name, int? excludeLabelId = null, CancellationToken ct = default);

    Task<List<Label>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);

    Task<Dictionary<int, int>> CountUsagesAsync(CancellationToken ct = default);
}
