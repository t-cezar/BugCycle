using BugCycle.Application.Authorization;
using BugCycle.Application.DTOs.Comments;
using BugCycle.Application.Interfaces;
using BugCycle.Application.Interfaces.Repositories;
using BugCycle.Application.Interfaces.Services;
using BugCycle.Application.Mapping;
using BugCycle.Domain.Entities;
using BugCycle.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace BugCycle.Application.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _comments;
    private readonly IIssueRepository _issues;
    private readonly IUserRepository _users;
    private readonly IProjectAccessGuard _guard;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        ICommentRepository comments,
        IIssueRepository issues,
        IUserRepository users,
        IProjectAccessGuard guard,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        ILogger<CommentService> logger)
    {
        _comments = comments;
        _issues = issues;
        _users = users;
        _guard = guard;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<CommentDto>> ListForIssueAsync(int issueId, CancellationToken ct = default)
    {
        var issue = await _issues.GetByIdAsync(issueId, ct)
            ?? throw new NotFoundException("Issue-ul", issueId);

        var permissions = await _guard.EnsureCanViewAsync(issue.ProjectId, ct);

        var comments = await _comments.ListForIssueAsync(issueId, ct);

        return comments.ToDtos(_currentUser.UserId, permissions.IsOwner);
    }

    public async Task<CommentDto> CreateAsync(
        int issueId,
        SaveCommentDto dto,
        CancellationToken ct = default)
    {
        var issue = await _issues.GetByIdAsync(issueId, ct)
            ?? throw new NotFoundException("Issue-ul", issueId);

        await _guard.EnsureCanCommentAsync(issue.ProjectId, ct);

        var userId = _currentUser.RequireUserId();

        var comment = new Comment
        {
            IssueId = issueId,
            AuthorId = userId,
            Body = dto.Body.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _comments.AddAsync(comment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Comentariu {CommentId} adaugat pe issue-ul {IssueId}.",
            comment.Id, issueId);

        var author = await _users.GetByIdAsync(userId, ct);
        comment.Author = author!;

        return comment.ToDto(canEdit: true, canDelete: true);
    }

    public async Task<CommentDto> UpdateAsync(
        int commentId,
        SaveCommentDto dto,
        CancellationToken ct = default)
    {
        var comment = await _comments.GetWithIssueAsync(commentId, ct)
            ?? throw new NotFoundException("Comentariul", commentId);

        await _guard.EnsureCanViewAsync(comment.Issue.ProjectId, ct);

        if (comment.AuthorId != _currentUser.RequireUserId())
        {
            throw new ForbiddenException("Poti edita doar propriile comentarii.");
        }

        comment.Body = dto.Body.Trim();
        comment.UpdatedAt = DateTime.UtcNow;

        _comments.Update(comment);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Comentariul {CommentId} a fost editat.", commentId);

        return comment.ToDto(canEdit: true, canDelete: true);
    }

    public async Task DeleteAsync(int commentId, CancellationToken ct = default)
    {
        var comment = await _comments.GetWithIssueAsync(commentId, ct)
            ?? throw new NotFoundException("Comentariul", commentId);

        var permissions = await _guard.EnsureCanViewAsync(comment.Issue.ProjectId, ct);

        var isAuthor = comment.AuthorId == _currentUser.RequireUserId();

        if (!isAuthor && !permissions.IsOwner)
        {
            throw new ForbiddenException(
                "Poti sterge doar propriile comentarii, daca nu esti Owner al proiectului.");
        }

        _comments.Remove(comment);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Comentariul {CommentId} a fost sters.", commentId);
    }
}
