using BugCycle.Application.DTOs.Users;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers.Api;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = AppRoles.Admin)]
[Produces("application/json")]
public class AdminUsersApiController : ControllerBase
{
    private readonly IUserAdminService _users;

    public AdminUsersApiController(IUserAdminService users) => _users = users;

    [HttpGet]
    [ProducesResponseType(typeof(List<UserWithRolesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<UserWithRolesDto>>> GetAll(CancellationToken ct)
        => Ok(await _users.ListAsync(ct));

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserWithRolesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserWithRolesDto>> GetById(string id, CancellationToken ct)
        => Ok(await _users.GetByIdAsync(id, ct));

    [HttpGet("/api/admin/roles")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<string>>> GetRoles(CancellationToken ct)
        => Ok(await _users.ListRolesAsync(ct));

    [HttpPost("{id}/roles")]
    [ProducesResponseType(typeof(UserWithRolesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserWithRolesDto>> AddRole(
        string id,
        [FromBody] RoleRequest request,
        CancellationToken ct)
        => Ok(await _users.AddRoleAsync(id, request.Role, ct));

    [HttpDelete("{id}/roles/{role}")]
    [ProducesResponseType(typeof(UserWithRolesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserWithRolesDto>> RemoveRole(
        string id,
        string role,
        CancellationToken ct)
        => Ok(await _users.RemoveRoleAsync(id, role, ct));

    public class RoleRequest
    {
        public string Role { get; set; } = string.Empty;
    }
}
