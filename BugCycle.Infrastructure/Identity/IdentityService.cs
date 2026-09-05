using BugCycle.Application.Interfaces;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Exceptions;
using BugCycle.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugCycle.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(string userId, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Utilizatorul", userId);

        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<Dictionary<string, List<string>>> GetRolesForUsersAsync(
        IEnumerable<string> userIds,
        CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<string, List<string>>();
        }

        var pairs = await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ids.Contains(ur.UserId))
            .Join(
                _context.Roles.AsNoTracking(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => new { ur.UserId, RoleName = r.Name! })
            .ToListAsync(ct);

        return pairs
            .GroupBy(p => p.UserId)
            .ToDictionary(g => g.Key, g => g.Select(p => p.RoleName).OrderBy(n => n).ToList());
    }

    public async Task<IReadOnlyList<string>> GetAllRolesAsync(CancellationToken ct = default)
        => await _context.Roles
            .AsNoTracking()
            .Select(r => r.Name!)
            .OrderBy(n => n)
            .ToListAsync(ct);

    public async Task AddToRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Utilizatorul", userId);

        if (!await _roleManager.RoleExistsAsync(role))
        {
            throw new DomainException($"Rolul '{role}' nu exista.");
        }

        if (await _userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(user, role);

        if (!result.Succeeded)
        {
            throw new DomainException(Describe(result));
        }
    }

    public async Task RemoveFromRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("Utilizatorul", userId);

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            return;
        }

        var result = await _userManager.RemoveFromRoleAsync(user, role);

        if (!result.Succeeded)
        {
            throw new DomainException(Describe(result));
        }
    }

    public async Task<int> CountUsersInRoleAsync(string role, CancellationToken ct = default)
    {
        var roleEntity = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == role, ct);

        if (roleEntity is null)
        {
            return 0;
        }

        return await _context.UserRoles.CountAsync(ur => ur.RoleId == roleEntity.Id, ct);
    }

    private static string Describe(IdentityResult result)
        => string.Join(" ", result.Errors.Select(e => e.Description));
}
