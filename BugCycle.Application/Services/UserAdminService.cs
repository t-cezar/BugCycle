using BugCycle.Application.DTOs.Users;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Mapping;
using BugCycle.Domain.Constants;
using BugCycle.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BugCycle.Application.Services;

public class UserAdminService : IUserAdminService
{
    private readonly IUserRepository _users;
    private readonly IProjectMemberRepository _members;
    private readonly IIdentityService _identity;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(
        IUserRepository users,
        IProjectMemberRepository members,
        IIdentityService identity,
        ICurrentUserService currentUser,
        ILogger<UserAdminService> logger)
    {
        _users = users;
        _members = members;
        _identity = identity;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<List<UserWithRolesDto>> ListAsync(CancellationToken ct = default)
    {
        var users = await _users.ListAllAsync(ct);
        var ids = users.Select(u => u.Id).ToList();

        var roles = await _identity.GetRolesForUsersAsync(ids, ct);
        var projectCounts = await _members.CountProjectsPerUserAsync(ids, ct);

        return users
            .Select(u => u.ToWithRolesDto(
                roles.TryGetValue(u.Id, out var r) ? r : new List<string>(),
                projectCounts.TryGetValue(u.Id, out var c) ? c : 0))
            .ToList();
    }

    public async Task<UserWithRolesDto> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("Utilizatorul", userId);

        var roles = await _identity.GetRolesAsync(userId, ct);
        var counts = await _members.CountProjectsPerUserAsync(new[] { userId }, ct);

        return user.ToWithRolesDto(roles, counts.TryGetValue(userId, out var c) ? c : 0);
    }

    public Task<IReadOnlyList<string>> ListRolesAsync(CancellationToken ct = default)
        => _identity.GetAllRolesAsync(ct);

    public async Task<UserWithRolesDto> AddRoleAsync(
        string userId,
        string role,
        CancellationToken ct = default)
    {
        _ = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("Utilizatorul", userId);

        await _identity.AddToRoleAsync(userId, role, ct);

        _logger.LogInformation(
            "Rolul {Role} a fost acordat utilizatorului {UserId} de catre {ActorId}.",
            role, userId, _currentUser.UserId);

        return await GetByIdAsync(userId, ct);
    }

    public async Task<UserWithRolesDto> RemoveRoleAsync(
        string userId,
        string role,
        CancellationToken ct = default)
    {
        _ = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("Utilizatorul", userId);

        if (role == AppRoles.Admin)
        {
            if (userId == _currentUser.UserId)
            {
                throw new DomainException(
                    "Nu iti poti retrage propriul rol de Admin. Cere altui administrator sa o faca.");
            }

            if (await _identity.CountUsersInRoleAsync(AppRoles.Admin, ct) <= 1)
            {
                throw new DomainException(
                    "Nu poti retrage rolul ultimului Admin din aplicatie.");
            }
        }

        if (role == AppRoles.User)
        {
            throw new DomainException(
                "Rolul de baza User nu poate fi retras; el garanteaza accesul la aplicatie.");
        }

        await _identity.RemoveFromRoleAsync(userId, role, ct);

        _logger.LogWarning(
            "Rolul {Role} a fost retras utilizatorului {UserId} de catre {ActorId}.",
            role, userId, _currentUser.UserId);

        return await GetByIdAsync(userId, ct);
    }
}
