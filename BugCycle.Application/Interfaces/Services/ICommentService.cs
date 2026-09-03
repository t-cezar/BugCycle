using BugCycle.Application.DTOs.Comments;

namespace BugCycle.Application.Interfaces.Services;

public interface ICommentService
{
    Task<List<CommentDto>> ListForIssueAsync(int issueId, CancellationToken ct = default);

    Task<CommentDto> CreateAsync(int issueId, SaveCommentDto dto, CancellationToken ct = default);

    Task<CommentDto> UpdateAsync(int commentId, SaveCommentDto dto, CancellationToken ct = default);

    Task DeleteAsync(int commentId, CancellationToken ct = default);
}
