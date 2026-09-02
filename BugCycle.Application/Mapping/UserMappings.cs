using BugCycle.Application.DTOs.Users;
using BugCycle.Domain.Entities;

namespace BugCycle.Application.Mapping;

public static class UserMappings
{
    public static UserSummaryDto ToSummaryDto(this ApplicationUser user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email ?? string.Empty,
        Initials = user.Initials
    };

    public static List<UserSummaryDto> ToSummaryDtos(this IEnumerable<ApplicationUser> users)
        => users.Select(u => u.ToSummaryDto()).ToList();

    public static UserWithRolesDto ToWithRolesDto(
        this ApplicationUser user,
        IEnumerable<string> roles,
        int projectCount) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email ?? string.Empty,
        Initials = user.Initials,
        CreatedAt = user.CreatedAt,
        Roles = roles.OrderBy(r => r).ToList(),
        ProjectCount = projectCount
    };
}
