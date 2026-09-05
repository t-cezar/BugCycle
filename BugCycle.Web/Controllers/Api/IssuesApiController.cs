using BugCycle.Application.DTOs.Issues;
using BugCycle.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers.Api;

[ApiController]
[Route("api/issues")]
[Produces("application/json")]
public class IssuesApiController : ControllerBase
{
    private readonly IIssueService _issues;

    public IssuesApiController(IIssueService issues) => _issues = issues;

    [HttpGet]
    [ProducesResponseType(typeof(List<IssueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<IssueDto>>> GetAll(
        [FromQuery] IssueFilter filter,
        CancellationToken ct)
        => Ok(await _issues.ListAsync(filter, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(IssueDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueDetailDto>> GetById(int id, CancellationToken ct)
        => Ok(await _issues.GetByIdAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(IssueDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IssueDto>> Create(
        [FromBody] CreateIssueDto dto,
        CancellationToken ct)
    {
        var created = await _issues.CreateAsync(dto, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(IssueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueDto>> Update(
        int id,
        [FromBody] UpdateIssueDto dto,
        CancellationToken ct)
        => Ok(await _issues.UpdateAsync(id, dto, ct));

    [HttpPatch("{id:int}/move")]
    [ProducesResponseType(typeof(IssueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IssueDto>> Move(
        int id,
        [FromBody] MoveIssueDto dto,
        CancellationToken ct)
        => Ok(await _issues.MoveAsync(id, dto, ct));

    [HttpPatch("{id:int}/sprint")]
    [ProducesResponseType(typeof(IssueDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IssueDto>> SetSprint(
        int id,
        [FromBody] SetSprintRequest request,
        CancellationToken ct)
        => Ok(await _issues.SetSprintAsync(id, request.SprintId, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _issues.DeleteAsync(id, ct);
        return NoContent();
    }

    public class SetSprintRequest
    {
        public int? SprintId { get; set; }
    }
}
