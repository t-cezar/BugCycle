using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Entities;
using BugCycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Repositories;

public class LabelRepository : Repository<Label>, ILabelRepository
{
    public LabelRepository(AppDbContext context) : base(context)
    {
    }

    public override async Task<List<Label>> ListAllAsync(CancellationToken ct = default)
        => await Context.Labels
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ToListAsync(ct);

    public async Task<bool> NameExistsAsync(
        string name,
        int? excludeLabelId = null,
        CancellationToken ct = default)
    {
        var query = Context.Labels.Where(l => l.Name == name);

        if (excludeLabelId.HasValue)
        {
            query = query.Where(l => l.Id != excludeLabelId.Value);
        }

        return await query.AnyAsync(ct);
    }

    public async Task<List<Label>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var list = ids.Distinct().ToList();

        if (list.Count == 0)
        {
            return new List<Label>();
        }

        return await Context.Labels
            .Where(l => list.Contains(l.Id))
            .ToListAsync(ct);
    }

    public async Task<Dictionary<int, int>> CountUsagesAsync(CancellationToken ct = default)
        => await Context.IssueLabels
            .AsNoTracking()
            .GroupBy(il => il.LabelId)
            .Select(g => new { LabelId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.LabelId, x => x.Count, ct);
}
