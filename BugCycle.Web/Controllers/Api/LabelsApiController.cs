using BugCycle.Application.DTOs.Labels;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers.Api;

[ApiController]
[Route("api/labels")]
[Produces("application/json")]
public class LabelsApiController : ControllerBase
{
    private readonly ILabelService _labels;

    public LabelsApiController(ILabelService labels) => _labels = labels;

    [HttpGet]
    [ProducesResponseType(typeof(List<LabelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LabelDto>>> GetAll(
        [FromQuery] bool includeUsage = false,
        CancellationToken ct = default)
        => Ok(await _labels.ListAsync(includeUsage, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(LabelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LabelDto>> GetById(int id, CancellationToken ct)
        => Ok(await _labels.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(LabelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LabelDto>> Create(
        [FromBody] SaveLabelDto dto,
        CancellationToken ct)
    {
        var created = await _labels.CreateAsync(dto, ct);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(typeof(LabelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LabelDto>> Update(
        int id,
        [FromBody] SaveLabelDto dto,
        CancellationToken ct)
        => Ok(await _labels.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _labels.DeleteAsync(id, ct);
        return NoContent();
    }
}
