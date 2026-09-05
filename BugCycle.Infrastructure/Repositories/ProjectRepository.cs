using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Entities;
using BugCycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<Project>> ListForUserAsync(
        string userId,
        bool includeArchived,
        CancellationToken ct = default)
    {
        var query = Context.Projects
            .AsNoTracking()
            .Where(p => p.Members.Any(m => m.UserId == userId));

        if (!includeArchived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        return await query
            .Include(p => p.CreatedBy)
            .Include(p => p.Members)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<List<Project>> ListAllWithCreatorAsync(
        bool includeArchived,
        CancellationToken ct = default)
    {
        var query = Context.Projects.AsNoTracking();

        if (!includeArchived)
        {
            query = query.Where(p => !p.IsArchived);
        }

        return await query
            .Include(p => p.CreatedBy)
            .Include(p => p.Members)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<Project?> GetWithMembersAsync(int id, CancellationToken ct = default)
        => await Context.Projects
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Project?> GetForDeletionAsync(int id, CancellationToken ct = default)
        => await Context.Projects
            .Include(p => p.Issues)
            .Include(p => p.Sprints)
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> KeyExistsAsync(
        string key,
        int? excludeProjectId = null,
        CancellationToken ct = default)
    {
        var query = Context.Projects.Where(p => p.Key == key);

        if (excludeProjectId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProjectId.Value);
        }

        return await query.AnyAsync(ct);
    }

    // Contoarele se calculeaza mereu prin interogare, niciodata stocate pe Project
    // ca sa nu poata ajunge desincronizate cu realitatea.
    public async Task<Dictionary<int, int>> CountIssuesPerProjectAsync(
        IEnumerable<int> projectIds,
        CancellationToken ct = default)
    {
        var ids = projectIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<int, int>();
        }

        return await Context.Issues
            .AsNoTracking()
            .Where(i => ids.Contains(i.ProjectId))
            .GroupBy(i => i.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Count, ct);
    }
}
