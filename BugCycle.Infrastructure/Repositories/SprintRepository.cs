using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;
using BugCycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Repositories;

public class SprintRepository : Repository<Sprint>, ISprintRepository
{
    public SprintRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Sprint>> ListForProjectAsync(int projectId, CancellationToken ct = default)
        => await Context.Sprints
            .AsNoTracking()
            .Where(s => s.ProjectId == projectId)
            .OrderBy(s => s.Status == SprintStatus.Active ? 0 : s.Status == SprintStatus.Planned ? 1 : 2)
            .ThenByDescending(s => s.StartDate)
            .ToListAsync(ct);

    public async Task<Sprint?> GetActiveForProjectAsync(int projectId, CancellationToken ct = default)
        => await Context.Sprints
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Status == SprintStatus.Active, ct);

    public async Task<Sprint?> GetWithIssuesAsync(int id, CancellationToken ct = default)
        => await Context.Sprints
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<bool> HasActiveSprintAsync(
        int projectId,
        int? excludeSprintId = null,
        CancellationToken ct = default)
    {
        var query = Context.Sprints
            .Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Active);

        if (excludeSprintId.HasValue)
        {
            query = query.Where(s => s.Id != excludeSprintId.Value);
        }

        return await query.AnyAsync(ct);
    }
}
