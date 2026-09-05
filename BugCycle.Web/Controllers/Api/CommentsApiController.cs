using BugCycle.Application.DTOs.Comments;
using BugCycle.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BugCycle.Web.Controllers.Api;

[ApiController]
[Route("api/issues/{issueId:int}/comments")]
[Produces("application/json")]
public class CommentsApiController : ControllerBase
{
    private readonly ICommentService _comments;

    public CommentsApiController(ICommentService comments) => _comments = comments;

    [HttpGet]
    [ProducesResponseType(typeof(List<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<CommentDto>>> GetAll(int issueId, CancellationToken ct)
        => Ok(await _comments.ListForIssueAsync(issueId, ct));

    [HttpPost]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CommentDto>> Create(
        int issueId,
        [FromBody] SaveCommentDto dto,
        CancellationToken ct)
    {
        var created = await _comments.CreateAsync(issueId, dto, ct);

        return CreatedAtAction(nameof(GetAll), new { issueId }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentDto>> Update(
        int issueId,
        int id,
        [FromBody] SaveCommentDto dto,
        CancellationToken ct)
        => Ok(await _comments.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int issueId, int id, CancellationToken ct)
    {
        await _comments.DeleteAsync(id, ct);
        return NoContent();
    }
}
