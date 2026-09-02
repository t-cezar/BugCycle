using BugCycle.Application.DTOs.Comments;
using BugCycle.Domain.Entities;

namespace BugCycle.Application.Mapping;

public static class CommentMappings
{
    public static CommentDto ToDto(
        this Comment comment,
        bool canEdit = false,
        bool canDelete = false) => new()
    {
        Id = comment.Id,
        IssueId = comment.IssueId,
        Author = comment.Author?.ToSummaryDto() ?? new(),
        Body = comment.Body,
        CreatedAt = comment.CreatedAt,
        UpdatedAt = comment.UpdatedAt,
        CanEdit = canEdit,
        CanDelete = canDelete
    };

    public static List<CommentDto> ToDtos(
        this IEnumerable<Comment> comments,
        string? currentUserId,
        bool currentUserIsProjectOwner) =>
        comments
            .OrderBy(c => c.CreatedAt)
            .Select(c =>
            {
                var isAuthor = currentUserId is not null && c.AuthorId == currentUserId;
                return c.ToDto(
                    canEdit: isAuthor,
                    canDelete: isAuthor || currentUserIsProjectOwner);
            })
            .ToList();
}
