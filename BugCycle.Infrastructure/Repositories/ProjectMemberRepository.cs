using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Enums;
using BugCycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Repositories;

public class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly AppDbContext _context;

    public ProjectMemberRepository(AppDbContext context) => _context = context;

    public async Task<ProjectMember?> GetAsync(
        int projectId,
        string userId,
        CancellationToken ct = default)
        => await _context.ProjectMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, ct);

    public async Task<ProjectRole?> GetRoleAsync(
        int projectId,
        string userId,
        CancellationToken ct = default)
    {
        var roles = await _context.ProjectMembers
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId && m.UserId == userId)
            .Select(m => (ProjectRole?)m.Role)
            .ToListAsync(ct);

        return roles.FirstOrDefault();
    }

    public async Task<List<ProjectMember>> ListForProjectAsync(
        int projectId,
        CancellationToken ct = default)
        => await _context.ProjectMembers
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.User.FullName)
            .ToListAsync(ct);

    public async Task<int> CountOwnersAsync(int projectId, CancellationToken ct = default)
        => await _context.ProjectMembers
            .CountAsync(m => m.ProjectId == projectId && m.Role == ProjectRole.Owner, ct);

    public async Task<Dictionary<string, int>> CountProjectsPerUserAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        return await _context.ProjectMembers
            .AsNoTracking()
            .Where(m => ids.Contains(m.UserId))
            .GroupBy(m => m.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count, ct);
    }

    public async Task AddAsync(ProjectMember member, CancellationToken ct = default)
        => await _context.ProjectMembers.AddAsync(member, ct);

    public void Update(ProjectMember member) => _context.ProjectMembers.Update(member);

    public void Remove(ProjectMember member) => _context.ProjectMembers.Remove(member);
}
