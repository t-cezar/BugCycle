using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.DTOs.Sprints;
using BugCycle.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers.Api;

[ApiController]
[Route("api/sprints")]
[Produces("application/json")]
public class SprintsApiController : ControllerBase
{
    private readonly ISprintService _sprints;

    public SprintsApiController(ISprintService sprints) => _sprints = sprints;

    [HttpGet]
    [ProducesResponseType(typeof(List<SprintDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SprintDto>>> GetAll(
        [FromQuery] int projectId,
        CancellationToken ct)
        => Ok(await _sprints.ListForProjectAsync(projectId, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SprintDto>> GetById(int id, CancellationToken ct)
        => Ok(await _sprints.GetByIdAsync(id, ct));

    [HttpGet("active")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<SprintDto>> GetActive(
        [FromQuery] int projectId,
        CancellationToken ct)
    {
        var active = await _sprints.GetActiveForProjectAsync(projectId, ct);

        return active is null ? NoContent() : Ok(active);
    }

    [HttpGet("{id:int}/issues")]
    [ProducesResponseType(typeof(List<IssueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<IssueDto>>> GetIssues(int id, CancellationToken ct)
        => Ok(await _sprints.ListBoardIssuesAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SprintDto>> Create(
        [FromQuery] int projectId,
        [FromBody] SaveSprintDto dto,
        CancellationToken ct)
    {
        var created = await _sprints.CreateAsync(projectId, dto, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SprintDto>> Update(
        int id,
        [FromBody] SaveSprintDto dto,
        CancellationToken ct)
        => Ok(await _sprints.UpdateAsync(id, dto, ct));

    [HttpPost("{id:int}/start")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SprintDto>> Start(int id, CancellationToken ct)
        => Ok(await _sprints.StartAsync(id, ct));

    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(SprintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SprintDto>> Complete(int id, CancellationToken ct)
        => Ok(await _sprints.CompleteAsync(id, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _sprints.DeleteAsync(id, ct);
        return NoContent();
    }
}
