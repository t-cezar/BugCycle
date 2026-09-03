using BugCycle.Application.Authorization;
using BugCycle.Application.DTOs.Projects;
using BugCycle.Application.DTOs.Users;

namespace BugCycle.Application.Interfaces.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> ListForCurrentUserAsync(bool includeArchived, CancellationToken ct = default);

    Task<List<ProjectDto>> ListAllForAdminAsync(bool includeArchived, CancellationToken ct = default);

    Task<ProjectDetailDto> GetByIdAsync(int id, CancellationToken ct = default);

    Task<ProjectDto> CreateAsync(SaveProjectDto dto, CancellationToken ct = default);

    Task<ProjectDto> UpdateAsync(int id, SaveProjectDto dto, CancellationToken ct = default);

    Task SetArchivedAsync(int id, bool archived, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<List<ProjectMemberDto>> ListMembersAsync(int projectId, CancellationToken ct = default);

    Task<ProjectMemberDto> AddMemberAsync(int projectId, AddProjectMemberDto dto, CancellationToken ct = default);

    Task<ProjectMemberDto> UpdateMemberRoleAsync(
        int projectId,
        string userId,
        UpdateProjectMemberRoleDto dto,
        CancellationToken ct = default);

    Task RemoveMemberAsync(int projectId, string userId, CancellationToken ct = default);

    Task<List<UserSummaryDto>> ListAssignableUsersAsync(int projectId, CancellationToken ct = default);

    Task<ProjectPermissions> GetPermissionsAsync(int projectId, CancellationToken ct = default);
}
