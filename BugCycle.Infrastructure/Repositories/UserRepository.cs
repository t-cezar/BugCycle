using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Domain.Entities;
using BugCycle.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToUpperInvariant();

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
    }

    public async Task<List<ApplicationUser>> ListAllAsync(CancellationToken ct = default)
        => await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

    public async Task<List<ApplicationUser>> ListByIdsAsync(
        IEnumerable<string> ids,
        CancellationToken ct = default)
    {
        var list = ids.Distinct().ToList();

        if (list.Count == 0)
        {
            return new List<ApplicationUser>();
        }

        return await _context.Users
            .AsNoTracking()
            .Where(u => list.Contains(u.Id))
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);
    }
}
