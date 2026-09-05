using BugCycle.Application.DTOs.Projects;
using BugCycle.Application.DTOs.Users;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers.Api;

[ApiController]
[Route("api/projects")]
[Produces("application/json")]
public class ProjectsApiController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsApiController(IProjectService projects) => _projects = projects;

    [HttpGet]
    [ProducesResponseType(typeof(List<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjectDto>>> GetAll(
        [FromQuery] bool includeArchived = false,
        CancellationToken ct = default)
        => Ok(await _projects.ListForCurrentUserAsync(includeArchived, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await _projects.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] SaveProjectDto dto,
        CancellationToken ct)
    {
        var created = await _projects.CreateAsync(dto, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> Update(
        int id,
        [FromBody] SaveProjectDto dto,
        CancellationToken ct)
        => Ok(await _projects.UpdateAsync(id, dto, ct));

    [HttpPost("{id:int}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        await _projects.SetArchivedAsync(id, true, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/unarchive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Unarchive(int id, CancellationToken ct)
    {
        await _projects.SetArchivedAsync(id, false, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _projects.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/members")]
    [ProducesResponseType(typeof(List<ProjectMemberDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ProjectMemberDto>>> GetMembers(int id, CancellationToken ct)
        => Ok(await _projects.ListMembersAsync(id, ct));

    [HttpPost("{id:int}/members")]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(
        int id,
        [FromBody] AddProjectMemberDto dto,
        CancellationToken ct)
    {
        var member = await _projects.AddMemberAsync(id, dto, ct);

        return CreatedAtAction(nameof(GetMembers), new { id }, member);
    }

    [HttpPut("{id:int}/members/{userId}")]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectMemberDto>> UpdateMemberRole(
        int id,
        string userId,
        [FromBody] UpdateProjectMemberRoleDto dto,
        CancellationToken ct)
        => Ok(await _projects.UpdateMemberRoleAsync(id, userId, dto, ct));

    [HttpDelete("{id:int}/members/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(int id, string userId, CancellationToken ct)
    {
        await _projects.RemoveMemberAsync(id, userId, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/assignable-users")]
    [ProducesResponseType(typeof(List<UserSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserSummaryDto>>> GetAssignableUsers(
        int id,
        CancellationToken ct)
        => Ok(await _projects.ListAssignableUsersAsync(id, ct));
}
